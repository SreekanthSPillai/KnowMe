'********************************************************************************
'***
'*** Module:        clsAscentXml
'*** Purpose:       Ascent Xml batch utilities
'***                Set the XML setup and runtime files for a batch, then
'***                use the functions in this class to process the batch.
'***
'*** (c) Copyright 2006 Kofax Image Products.
'*** All rights reserved.
'***
'********************************************************************************

Option Strict Off
Option Explicit On 
Imports Kofax.MSXML.Interop


Friend Class clsAscentXml

	'*** Enumerations
	Public Enum acxXmlType
		acxRuntimeType
		acxSetupType
		acxDocumentType
	End Enum
	
	'*** Class members
    Private m_oXSetupDocument As DOMDocument
    Private m_oXRuntimeDocument As DOMDocument
    Private m_oXDocDocument As DOMDocument
	Private m_strImagesPath As String '*** Images directory for the batch.
	'*** Cached for performance.

    '**************************************************************************
    '*** Property:  XmlSetupFile
    '*** Purpose:   Set the path to the XML file that represents the batch
    '**************************************************************************
    Public WriteOnly Property XmlSetupFile() As String
        Set(ByVal Value As String)
            m_oXSetupDocument = OpenXmlFile(Value)
        End Set
    End Property

    '**************************************************************************
    '*** Property:  XmlSetup
    '*** Purpose:   Returns the DOM document.
    '*** Notes:     The XmlSetupFile must be set first.
    '**************************************************************************
    Public ReadOnly Property XmlSetup() As DOMDocument
        Get
            XmlSetup = m_oXSetupDocument
        End Get
    End Property

    '**************************************************************************
    '*** Property:  XmlRuntimeFile
    '*** Purpose:   Set the path to the XML file that represents the batch
    '**************************************************************************
    Public WriteOnly Property XmlRuntimeFile() As String
        Set(ByVal Value As String)
            m_oXRuntimeDocument = OpenXmlFile(Value)
        End Set
    End Property

    '**************************************************************************
    '*** Property:  XmlRuntime
    '*** Purpose:   Returns the DOM document.
    '*** Notes:     The XmlRuntimeFile must be set first.
    '**************************************************************************
    Public ReadOnly Property XmlRuntime() As DOMDocument
        Get
            XmlRuntime = m_oXRuntimeDocument
        End Get
    End Property
    '**************************************************************************
    '*** Property:  XmlRuntimeDocumentsFile
    '*** Purpose:   Set the path to the XML file that represents the documents.
    '**************************************************************************
    Public WriteOnly Property XmlRuntimeDocumentsFile() As String
        Set(ByVal Value As String)
            m_oXDocDocument = OpenXmlFile(Value)
        End Set
    End Property

    '**************************************************************************
    '*** Property:  XmlDocument
    '*** Purpose:   Returns the DOM document.
    '*** Notes:     The XmlDocumentFile must be set first.
    '**************************************************************************
    Public ReadOnly Property XmlRuntimeDocuments() As DOMDocument
        Get
            XmlRuntimeDocuments = m_oXDocDocument
        End Get
    End Property

    '**************************************************************************
    '*** Function:  WriteXmlRuntimeFile
    '*** Purpose:   Write the batch XML file, based on changes to the DOM
    '**************************************************************************
    Public Sub WriteXmlRuntimeFile(ByVal strFile As String)

        Call SaveXmlFile(strFile, m_oXRuntimeDocument)

    End Sub

    '**************************************************************************
    '*** Function:  GetXml
    '*** Purpose:   Returns a DOM document absed on acxXmlType.
    '*** Notes:     The XmlDocumentFile must be set first.
    '**************************************************************************
    Public Function GetXml(ByVal XmlType As acxXmlType) As DOMDocument
        Select Case XmlType
            Case acxXmlType.acxRuntimeType
                GetXml = XmlRuntime
            Case acxXmlType.acxSetupType
                GetXml = XmlSetup
            Case acxXmlType.acxDocumentType
                GetXml = XmlRuntimeDocuments
            Case Else
                Throw New System.ApplicationException("Unknown XmlType")
        End Select
    End Function

    '**************************************************************************
    '*** Function:  OpenXmlFile
    '*** Purpose:   Open an Xml file into a DOM object
    '*** Input:     strFileName - Path of XML file
    '*** Output:    Returns the DOMDocument
    '*************************************************************************
    Private Function OpenXmlFile(ByVal strFileName As String) _
    As DOMDocument

        Dim oXDocument As New DOMDocument

        '*** VERY IMPORTANT to make reading synchronous to insure
        '*** that the file is not changing
        oXDocument.async = False

        '*** Load the XML file
        oXDocument.load(strFileName)
        '*** Throw an error if the file has not been read

        Dim oXParseError As IXMLDOMParseError
        Dim strParseError As String
        If Not oXDocument.hasChildNodes Then
            oXParseError = oXDocument.parseError
            strParseError = " (" & oXParseError.reason & " " & _
            oXParseError.line & ")"
            Throw New System.ApplicationException("Parse Error" & _
                strParseError)
        End If

        OpenXmlFile = oXDocument
    End Function

    '**************************************************************************
    '*** Function:  SaveXmlFile
    '*** Purpose:   Write the Xml file for a DOM object
    '*** Input:     strFileName - Path of XML file
    '***            oDocument - the DOM representing the XML to write
    '**************************************************************************
    Private Sub SaveXmlFile(ByVal strFileName As String, _
    ByRef oXDocument As DOMDocument)
        oXDocument.save((strFileName))
    End Sub

    '**************************************************************************
    '*** Function:  GetDocuments
    '*** Purpose:   Get the collection of documents from the Xml Batch object
    '*** Input:     None
    '*** Output:    Returns a node list of all "Document" objects
    '**************************************************************************
    Public Function GetDocuments() As IXMLDOMNodeList
        If XmlRuntimeDocuments Is Nothing Then
            GetDocuments = GetBatch().selectNodes("Documents/Document")
        Else
            GetDocuments = XmlRuntimeDocuments.selectNodes _
                            ("//Documents/Document")
        End If
    End Function

    '**************************************************************************
    '*** Function:  GetBatch
    '*** Purpose:   Get the collection of documents from the Xml Batch object
    '*** Input:     None
    '*** Output:    Returns the batch node
    '**************************************************************************
    Public Function GetBatch() As IXMLDOMNode
        GetBatch = XmlRuntime.selectSingleNode("//Batch")
    End Function

    '**************************************************************************
    '*** Function:  GetLoosePages
    '*** Purpose:   Get the collection of loose pages from the Xml Batch object
    '*** Input:     None
    '*** Output:    Returns a node list of all loose "Page" objects
    '**************************************************************************
    Public Function GetLoosePages() As IXMLDOMNodeList
        GetLoosePages = GetBatch().selectNodes("Pages/Page")
    End Function

    '**************************************************************************
    '*** Function:  GetLoosePagesNode
    '*** Purpose:   Get the loose pages node from the Xml Batch object
    '*** Input:     None
    '*** Output:    Returns the loose pages
    '**************************************************************************
    Public Function GetLoosePagesNode() As IXMLDOMNode
        Dim oNode As IXMLDOMNode
        oNode = XmlRuntime.selectSingleNode("//Batch/Pages")
        Dim oBatchNode As IXMLDOMNode

        If oNode Is Nothing Then
            oBatchNode = GetBatch()
            oNode = CreateElement("Pages")
            Call oBatchNode.appendChild(oNode)
        End If
        GetLoosePagesNode = oNode
    End Function

    '**************************************************************************
    '*** Function:  GetDocumentsNode
    '*** Purpose:   Get the document node from the Xml Batch object
    '*** Input:     None
    '*** Output:    Returns the documents
    '**************************************************************************
    Public Function GetDocumentsNode() As IXMLDOMNode
        Dim oNode As IXMLDOMNode
        oNode = XmlRuntime.selectSingleNode("//Batch/Documents")
        Dim oBatchNode As IXMLDOMNode
        Dim oPages As IXMLDOMNode

        If oNode Is Nothing Then
            oBatchNode = GetBatch()
            oNode = CreateElement("Documents")
            oPages = GetLoosePagesNode()
            Call oBatchNode.insertBefore(oNode, oPages)
        End If
        GetDocumentsNode = oNode
    End Function

    '**************************************************************************
    '*** Function:  SetAttribute
    '*** Purpose:   Sets an attribute in an element node
    '*** Input:     oXDocumentNode is the document node
    '***            strName - Name of the attribute to set
    '***            strValue - Value to set
    '***            DocType - indicates which Xml doc to modify
    '*** Output:    None
    '**************************************************************************
    Public Sub SetAttribute(ByRef oXElement As IXMLDOMNode, _
    ByVal strName As String, ByVal strValue As String, _
    Optional ByRef XmlType As acxXmlType = acxXmlType.acxRuntimeType)

        '*** Get the attribute map		
        Dim oXAttrList As IXMLDOMNamedNodeMap
        oXAttrList = oXElement.attributes

        '*** Create the new attribute node and set the value		
        Dim oXAttribute As IXMLDOMNode
        oXAttribute = GetXml(XmlType).createAttribute(strName)
        oXAttribute.nodeValue = strValue
        oXAttrList.setNamedItem(oXAttribute)
    End Sub

    '**************************************************************************
    '*** Function:  CreateElement
    '*** Purpose:   Creates the specified XML element.
    '*** Input:     strElement - Element to create
    '*** Output:    The element that was created.
    '**************************************************************************
    Public Function CreateElement(ByVal strElement As String) _
    As IXMLDOMElement
        CreateElement = m_oXRuntimeDocument.createElement(strElement)
    End Function

    '**************************************************************************
    '*** Function:  GetImageFile
    '*** Purpose:   Returns an image file path based on the image path and 
    '***            image id
    '*** Input:     oXPage - Page node
    '*** Output:    Outputs the actual file path
    '**************************************************************************
	Public Function GetImageFile(ByRef oXPage As IXMLDOMNode, _
	ByRef oBatch As Kofax.Capture.DBLite.Batch) As String

		'*** Check to see if this file has an extension, then use it
		'*** to get the image ID from the page
		Dim oExtNode As IXMLDOMNode
		oExtNode = oXPage.attributes.getNamedItem("Extension")

		If Not oExtNode Is Nothing Then
			GetImageFile = oBatch.ImageFileWithExt _
			 (oXPage.attributes.getNamedItem("ImageID").nodeValue, _
			 oExtNode.text)
		Else
			GetImageFile = oBatch.ImageFile _
			 (oXPage.attributes.getNamedItem("ImageID").nodeValue)
		End If
	End Function
End Class