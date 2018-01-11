# MIT License
#
# Copyright (c) 2017, Kayac Inc.
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is furnished
# to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
# WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
# CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

__copyright__ = "Copyright (c) 2017, Kayac Inc."
__license__ = "MIT"
__version__ = "1.0.0"
__author__ = "Affath Firdausi"
__email__ = "affath-firdausi@kayac.com"

import maya.cmds as cmds
import maya.api.OpenMaya as om
import maya.api.OpenMayaUI as omui
import maya.OpenMaya as mo
import math
import uuid
from functools import partial

# Constants
R_CHANNEL = "R"
G_CHANNEL = "G"
B_CHANNEL = "B"
A_CHANNEL = "A"
MAIN_COLOR_SET = "RGBA"
RESERVED_SETS = [R_CHANNEL, G_CHANNEL, B_CHANNEL, A_CHANNEL, MAIN_COLOR_SET]
SUB_COLOR_SETS = [R_CHANNEL, G_CHANNEL, B_CHANNEL, A_CHANNEL]
COLOR_SET_NAMES = {
	R_CHANNEL : "red",
	G_CHANNEL : "green",
	B_CHANNEL : "blue",
	A_CHANNEL : "white",
	MAIN_COLOR_SET : "white"}
COLOR_SET_COLORS = {
	R_CHANNEL : (1, 0, 0, 1),
	G_CHANNEL : (0, 1, 0, 1),
	B_CHANNEL : (0, 0, 1, 1),
	A_CHANNEL : (1, 1, 1, 1),
	MAIN_COLOR_SET : (1, 1, 1, 1)}
SELECTION_LABEL_HANDLE = "Selection"
CHANNEL_LABEL_HANDLE = "Channel"
COMBINE_ONLY_BUTTON_HANDLE = "CombineOnly"
COMBINE_SWITCH_BUTTON_HANDLE = "CombineSwitch"
IMPORT_BUTTON_HANDLE = "Import"
EXPORT_BUTTON_HANDLE = "Export"
EXPORT_NOTE1_HANDLE = "ExportNote1"
EXPORT_NOTE2_HANDLE = "ExportNote2"
SHOW_TOOL_BUTTON_HANDLE = "ShowTool"
VERSION_LABEL_HANDLE = "Version"
RESET_BUTTON_HANDLE = "Reset"
FILL_BUTTON_HANDLE = "Fill"
WINDOW_WIDTH = 272
WIN_ID = "VertexColorChannelSelector"
FILE_FILTER = "TIFF Files (*.TIFF);;JPEG Files (*.JPEG);;PNG Files (*.PNG)"

"""
VertexColorChannelSelector is a tool for selecting and painting one specific channel
of vertex color's four RGBA channels. It works by splitting existing main vertex color
set into four vertex color sets which then the user can paint on each of the color sets
as one color channel, and combine them into main vertex color set when finished.
"""
class VertexColorChannelSelector(object):

	uiElmMaps = {}

	def __init__(self):
		self.showWindow()

	def getButton(self, buttonName):
		return self.getUiElm(cmds.button, buttonName)

	def getLabel(self, labelName):
		return self.getUiElm(cmds.text, labelName)

	def getUiElm(self, elmGenFunc, uiElmName):
		elmId = None
		uiElmMaps = self.uiElmMaps
		if not uiElmMaps.has_key(uiElmName):
			elmId = elmGenFunc(uiElmName)
			uiElmMaps[uiElmName] = elmId
		else:
			elmId = uiElmMaps[uiElmName]
		return elmId

	def showWindow(self):

		getLabel = self.getLabel
		getButton = self.getButton

		if cmds.window(WIN_ID, query = True, exists = True):
			cmds.deleteUI(WIN_ID)

		window = cmds.window(WIN_ID)
		cmds.columnLayout()
		cmds.text(getLabel("SelectionText"), e=True, label="Selection(s):", font="boldLabelFont", width=WINDOW_WIDTH, align="left")
		cmds.text(getLabel(SELECTION_LABEL_HANDLE), e=True, label=getSelectionsString())

		cmds.setParent('..')
		cmds.rowLayout(numberOfColumns=6, columnWidth6=(140, 20, 20, 20, 20, 45))
		cmds.text(getLabel(CHANNEL_LABEL_HANDLE), e=True, label="Channel:", font="boldLabelFont")
		cmds.button(getButton(R_CHANNEL), e=True, label='R', command=partial(self.switchChannel, R_CHANNEL))
		cmds.button(getButton(G_CHANNEL), e=True, label='G', command=partial(self.switchChannel, G_CHANNEL))
		cmds.button(getButton(B_CHANNEL), e=True, label='B', command=partial(self.switchChannel, B_CHANNEL))
		cmds.button(getButton(A_CHANNEL), e=True, label='A', command=partial(self.switchChannel, A_CHANNEL))
		cmds.button(getButton(MAIN_COLOR_SET), e=True, label='RGBA', command=partial(self.switchChannel, MAIN_COLOR_SET))

		cmds.setParent('..')
		cmds.button(getButton(SHOW_TOOL_BUTTON_HANDLE), e=True, label="Show Paint Vertex Color Tool", command=showPaintTool, width=WINDOW_WIDTH)
		cmds.button(getButton(FILL_BUTTON_HANDLE), e=True, label="Fill color", command=resetToColor, width=WINDOW_WIDTH)
		cmds.button(getButton(RESET_BUTTON_HANDLE), e=True, label="Clear color", command=resetToBlack, width=WINDOW_WIDTH)
		cmds.button(getButton(COMBINE_ONLY_BUTTON_HANDLE), e=True, label="Apply", command=partial(self.combineChannels, False), width=WINDOW_WIDTH)
		cmds.button(getButton(COMBINE_SWITCH_BUTTON_HANDLE), e=True, label="Apply and View Result", command=partial(self.combineChannels, True), width=WINDOW_WIDTH)
		cmds.button(getButton(IMPORT_BUTTON_HANDLE), e=True, label="Import Texture", command=partial(self.doImportTexture, False), width=WINDOW_WIDTH)
		cmds.button(getButton(EXPORT_BUTTON_HANDLE), e=True, label="Export Texture", command=exportTexture, width=WINDOW_WIDTH)
		cmds.text(getLabel(EXPORT_NOTE1_HANDLE), e=True, align="left", label="Import/Export Texture settings are located in", font="tinyBoldLabelFont", width=WINDOW_WIDTH)
		cmds.setParent('..')
		cmds.rowLayout(numberOfColumns=2, columnWidth2=(WINDOW_WIDTH - 30, 30))
		cmds.text(getLabel(EXPORT_NOTE2_HANDLE), e=True, align="left", label="Paint Vertex Color Tool", font="tinyBoldLabelFont")
		cmds.text(getLabel(VERSION_LABEL_HANDLE), e=True, align="right", label="v" + __version__, font="tinyBoldLabelFont")
		cmds.setParent('..')
		cmds.showWindow(window)

		self.updateUi()
		self.selectionChangeCallbackId = om.MEventMessage.addEventCallback("SelectionChanged", self.onSelectionChanged)
		omui.MUiMessage.addUiDeletedCallback(window, self.onWindowClosed)

	def onSelectionChanged(self, args):
		self.updateUi()
		validateChannels()

	def onWindowClosed(self, args):
		if self.selectionChangeCallbackId != None:
			om.MMessage.removeCallback(self.selectionChangeCallbackId)

	def updateChannelButtons(self, channel):
		validChannel = channel in RESERVED_SETS
		for button in RESERVED_SETS:
			buttonId = self.getButton(button)
			splitIndex = len(buttonId) - buttonId.rfind('|')
			buttonChannel = buttonId[1-splitIndex:]
			cmds.button(buttonId, e=True, enable=not validChannel or not buttonChannel == channel)

	def updateUi(self):
		getLabel = self.getLabel
		getButton = self.getButton

		selectionChannels = getCurrentColorSetNames()
		selectedChannel = "N/A"
		if len(selectionChannels) > 1:
			self.updateChannelButtons("")
			selectedChannel = "Mixed"
		elif len(selectionChannels) == 1:
			self.updateChannelButtons(selectionChannels[0])
			selectedChannel = selectionChannels[0]
		else:
			validateChannels()


		selectedObjects = getSelectionsString()
		selectedMeshCount = getSelectedMeshCount()

		cmds.text(getLabel(CHANNEL_LABEL_HANDLE), e=True, label="Channel: "+selectedChannel, font="boldLabelFont")
		cmds.text(getLabel(SELECTION_LABEL_HANDLE), e=True, label=selectedObjects)

		validChannel = len(selectionChannels) == 1
		hasSelectedObject = selectedObjects != None and selectedObjects != "None"
		cmds.button(getButton(COMBINE_ONLY_BUTTON_HANDLE), e=True, enable=hasSelectedObject)
		cmds.button(getButton(COMBINE_SWITCH_BUTTON_HANDLE), e=True, enable=hasSelectedObject)
		cmds.button(getButton(IMPORT_BUTTON_HANDLE), e=True, label="Import Texture" + ( " (" + selectedChannel + ")" if validChannel else ""), enable=hasSelectedObject and selectedMeshCount == 1 and validChannel)
		cmds.button(getButton(EXPORT_BUTTON_HANDLE), e=True, label="Export Texture" + ( " (" + selectedChannel + ")" if validChannel else ""), enable=hasSelectedObject and validChannel)
		cmds.button(getButton(RESET_BUTTON_HANDLE), e=True, label="Clear color" + ( " (" + selectedChannel + ")" if validChannel else ""), enable=hasSelectedObject and validChannel)
		cmds.button(getButton(FILL_BUTTON_HANDLE), e=True, label="Fill color" + ( " (" + selectedChannel + ")" if validChannel else ""), enable=hasSelectedObject and validChannel)

	def switchChannel(self, channel, *args):
		if not isExistsColorSet(channel):
			validateChannels()
		iter = getMeshIter()
		for mesh in iter:
			mesh.setCurrentColorSetName(channel)
		self.updateUi()

	def combineChannels(self, switchToMain, *args):
		currentChannel = None
		for mesh in getMeshIter():
			currentChannel = mesh.currentColorSetName()

		validateChannels()

		iter = getMeshIter()
		# combine SUB_COLOR_SETS' into MAIN_COLOR_SET
		for mesh in iter:
			rColors = mesh.getVertexColors(R_CHANNEL)
			gColors = mesh.getVertexColors(G_CHANNEL)
			bColors = mesh.getVertexColors(B_CHANNEL)
			aColors = mesh.getVertexColors(A_CHANNEL)

			colors = om.MColorArray(mesh.numVertices, om.MColor((1, 1, 1, 1)))
			for i in xrange(mesh.numVertices):
				colors[i] = om.MColor((rColors[i].r, gColors[i].g, bColors[i].b, aColors[i].r))

			mesh.setCurrentColorSetName(MAIN_COLOR_SET)
			mesh.setVertexColors(colors, getIncrementIter(mesh.numVertices))
			mesh.syncObject()

		if switchToMain:
			currentChannel = MAIN_COLOR_SET

		if currentChannel != None:
			for mesh in getMeshIter():
				mesh.setCurrentColorSetName(currentChannel)
			self.updateUi()
		# delete all color sets except MAIN_COLOR_SET

	def doImportTexture(self, *args):
		currentColorSet = getCurrentColorSetNames()[0]
		importTexture(args)
		self.switchChannel(currentColorSet)

######################

# Get selected object list in string
def getSelectionsString():
	selections = None
	for item in getSelectionList():
		if (selections == None):
			selections = str(item)
		else:
			selections = selections + ", " + item

	if (selections == None or len(selections) == 0):
		return "None"
	elif (len(selections) > 40):
		return selections[:40] + "..."
	else:
		return selections

# Get selected object list
def getSelectionList():
	iter = om.MItSelectionList(om.MGlobal.getActiveSelectionList())
	selections = list()
	while not iter.isDone():
		selections.append(str(iter.getDagPath()))
		iter.next()

	return selections

def resetToBlack(args):
	fillColor((0, 0, 0, 1), "black")

def resetToColor(args):
	fillColor((1, 1, 1, 1), None)

# Reset vertex color to black
def fillColor(color, colorName):
	validateChannels()
	channel = getCurrentColorSetNames()[0]
	colorName = COLOR_SET_NAMES[channel] if colorName == None else colorName
	warnMessage = "Fill %s channel's color to %s?" % (channel, colorName)
	resetAll = channel == MAIN_COLOR_SET
	if resetAll:
		warnMessage = "Fill all channels color to %s?" % (colorName)

	result = cmds.confirmDialog(
		title='Warning',
		message=warnMessage,
		button=['OK', 'Cancel'],
		defaultButton='OK',
		cancelButton='Cancel',
		dismissString='Cancel')

	if result != "OK":
		return

	for mesh in getMeshIter():
		colors = mesh.getVertexColors(MAIN_COLOR_SET)
		numColors = len(colors)
		if resetAll:
			for colorSet in RESERVED_SETS:
				defaultColor = COLOR_SET_COLORS[colorSet]
				newColor = [a * b for a, b in zip(color, defaultColor)]
				newColors = om.MColorArray(numColors, om.MColor(newColor))
				mesh.setCurrentColorSetName(colorSet)
				mesh.setVertexColors(newColors, getIncrementIter(numColors))
		else:
			defaultColor = COLOR_SET_COLORS[channel]
			newColor = [a * b for a, b in zip(color, defaultColor)]
			newColors = om.MColorArray(numColors, om.MColor(newColor))
			mesh.setVertexColors(newColors, getIncrementIter(numColors))

# Show Maya PaintVertexColorTool UI
def showPaintTool(args):
	maya.mel.eval("PaintVertexColorTool;")

# Open file dialog to import texture as vertex color
def importTexture(args):
	inFile = cmds.fileDialog2(fileFilter=FILE_FILTER, dialogStyle=2, fileMode=1)
	if inFile == None or len(inFile) == 0:
		return
	inFile = str(inFile[0].encode('utf8'))
	temp = inFile.split(".")
	inFileType = temp[len(temp)-1]
	loadTexture(inFile, inFileType)

# Load texture and assign into vertex color
def loadTexture(fileName, fileType):
	if (fileName == None or len(fileName) == 0):
		return
	maya.mel.eval("PaintVertexColorTool;")
	context = cmds.currentCtx()
	cmds.artAttrPaintVertexCtx(context, whichTool="colorPerVertex", e=True, ifm="rgba", ifl=fileName)

	mesh = getMesh()

	if isMainColorSet():
		subColorSets = SUB_COLOR_SETS
		colors = mesh.getVertexColors(MAIN_COLOR_SET)
	else:
		subColorSets = [mesh.currentColorSetName()]
		colors = mesh.getVertexColors(subColorSets[0])

	# filter unused channel
	for subColorSet in subColorSets:
		mesh.setCurrentColorSetName(subColorSet)
		useR = int(subColorSet == R_CHANNEL or subColorSet == MAIN_COLOR_SET)
		useG = int(subColorSet == G_CHANNEL or subColorSet == MAIN_COLOR_SET)
		useB = int(subColorSet == B_CHANNEL or subColorSet == MAIN_COLOR_SET)
		useA = int(subColorSet == A_CHANNEL)

		numColors = len(colors)
		subColors = om.MColorArray(numColors, om.MColor((1.0, 1.0, 1.0, 1.0)))
		for i in xrange(numColors):
			color = colors[i]
			subColors[i] = om.MColor((
				color.r * useR + color.a * useA,
				color.g * useG + color.a * useA,
				color.b * useB + color.a * useA,
				1))
		mesh.setVertexColors(subColors, getIncrementIter(numColors))

# Open file dialog to export vertex color as texture
def exportTexture(args):
	outFile = cmds.fileDialog2(fileFilter=FILE_FILTER, dialogStyle=2, fileMode=0)
	if outFile == None or len(outFile) == 0:
		return
	outFile = str(outFile[0].encode('utf8'))
	temp = outFile.split(".")
	outFileType = temp[len(temp)-1]
	if getSelectedMeshCount() > 1:
		selectedChannel = getCurrentColorSetNames()[0]
	else:
		selectedChannel = ""
	saveTexture(outFile[:outFile.rfind(".")] + selectedChannel + "." + outFileType, outFile[outFile.rfind(".")+1:])

# Save vertex color as texture
def saveTexture(fileName, fileType):
	maya.mel.eval("PaintVertexColorTool;")
	context = cmds.currentCtx()
	cmds.artAttrPaintVertexCtx(context, whichTool="colorPerVertex", e=True, efm='rgba', eft=fileType, esf=fileName)

# Validate vertex color channels
def validateChannels():
	# create MAIN_COLOR_SET if not exist: rename existing color set or create a new one if no color sets defined

	for mesh in getMeshIter():
		currentColorSet = mesh.currentColorSetName()
		colorSets = mesh.getColorSetNames()
		if colorSets == None or not MAIN_COLOR_SET in colorSets:
			mainColors = om.MColorArray(mesh.numVertices, om.MColor((1.0, 1.0, 1.0, 1.0)))
			mesh.createColorSet(MAIN_COLOR_SET, True)
			mesh.setColors(mainColors, MAIN_COLOR_SET)
			mesh.setCurrentColorSetName(MAIN_COLOR_SET)
			mesh.setVertexColors(mainColors, getIncrementIter(mesh.numVertices))
			mesh.syncObject()

		mainColors = None

		for subColorSet in SUB_COLOR_SETS:
			if colorSets == None or not subColorSet in colorSets:
				if mainColors == None:
					mainColors = mesh.getVertexColors(MAIN_COLOR_SET)
				numColors = len(mainColors)
				mesh.createColorSet(subColorSet, True)
				useR = int(subColorSet == R_CHANNEL or subColorSet == A_CHANNEL)
				useG = int(subColorSet == G_CHANNEL or subColorSet == A_CHANNEL)
				useB = int(subColorSet == B_CHANNEL or subColorSet == A_CHANNEL)

				subColors = om.MColorArray(numColors, om.MColor((1.0, 1.0, 1.0, 1.0)))
				for i in xrange(numColors):
					color = mainColors[i]
					subColors[i] = om.MColor((color.r * useR, color.g * useG, color.b * useB, 1))

				mesh.setCurrentColorSetName(subColorSet)
				mesh.setVertexColors(subColors, getIncrementIter(numColors))


		# make sure color_sets[0] is MAIN_COLOR_SET
		colorSets = mesh.getColorSetNames()
		if colorSets[0] != MAIN_COLOR_SET:
			otherSet = colorSets[0]
			aBuff = mesh.getVertexColors(otherSet)
			bBuff = mesh.getVertexColors(MAIN_COLOR_SET)
			mesh.setCurrentColorSetName(otherSet)
			mesh.setVertexColors(bBuff, getIncrementIter(len(bBuff)))
			mesh.setCurrentColorSetName(MAIN_COLOR_SET)
			mesh.setVertexColors(aBuff, getIncrementIter(len(aBuff)))
			mesh.syncObject()
			cmds.polyColorSet(rename=True, colorSet=otherSet, newColorSet="__TEMP")
			cmds.polyColorSet(rename=True, colorSet=MAIN_COLOR_SET, newColorSet=otherSet)
			cmds.polyColorSet(rename=True, colorSet="__TEMP", newColorSet=MAIN_COLOR_SET)

		if (currentColorSet == None or len(currentColorSet) == 0):
			mesh.setCurrentColorSetName(R_CHANNEL)
		else:
			mesh.setCurrentColorSetName(currentColorSet)

def isExistsColorSet(channel):
	iter = getMeshIter()
	for mesh in iter:
		colorSets = mesh.getColorSetNames()
		return colorSets != None and channel in colorSets
	return False

def isMainColorSet():
	colorSets = getCurrentColorSetNames()
	return len(colorSets) == 1 and MAIN_COLOR_SET in colorSets

def getIncrementIter(end, start = 0):
	cur = start
	while cur < end:
		yield cur
		cur += 1

def getMeshIter():
	iter = om.MItSelectionList(om.MGlobal.getActiveSelectionList())
	while not iter.isDone():
		dagPath = iter.getDagPath()
		dagPathStr = dagPath.fullPathName()
		shapes = cmds.listRelatives(dagPathStr, shapes=True)
		if (shapes == None or len(shapes) == 0) and dagPath.childCount() > 0:
			for mesh in getMeshIterFromGroup(dagPath):
				yield mesh
		else:
			dagPath.extendToShape()
			mesh = om.MFnMesh(dagPath)
			mesh.syncObject()
			yield mesh
		iter.next()

def getMesh():
	for mesh in getMeshIter():
		return mesh
	return None

def getSelectedMeshCount():
	count = 0
	for mesh in getMeshIter():
		count += 1
	return count

def getMeshIterFromGroup(dagPath):
	dagPathStr = dagPath.fullPathName()
	childCount = dagPath.childCount()
	for ci in xrange(childCount):
		child = dagPath.child(ci)
		if not child.hasFn(mo.MFn.kDagNode):
			continue
		childDag = om.MDagPath.getAPathTo(child)
		shapes = cmds.listRelatives(childDag.fullPathName(), shapes=True)
		if (shapes == None or len(shapes) == 0) and childDag.childCount() > 0:
			for mesh in getMeshIterFromGroup(childDag):
				yield mesh
		else:
			childDag.extendToShape()
			mesh = om.MFnMesh(childDag)
			mesh.syncObject()
			yield mesh
	return

def getCurrentColorSetNames():
	iter = getMeshIter()
	colorSetNames = set()
	for mesh in iter:
		colorSetNames.add(mesh.currentColorSetName())
	return list(colorSetNames)

VertexColorChannelSelector()