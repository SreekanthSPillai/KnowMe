'******************************************************************************
'***
'*** Module:    BatchMan.vb - BatchManager
'*** Purpose:   Encapsulates handling of batch selection and locking.
'***
'*** (c) Copyright 2006 Kofax Image Products.
'*** All rights reserved.
'***
'******************************************************************************

Imports System.Runtime.InteropServices
Imports System.Diagnostics
Imports Kofax.Capture.DBLite
Imports Microsoft.Win32
Imports System.IO
Imports Kofax.Capture.SDK.CustomModule
Imports Kofax.Capture.SDK.Data

Friend Class BatchManager
	Private Const THIS_FILE As String = "BatchManager"

	'*** Enumerations
	Public Enum bmProcessMode
		bmProcessByBatch
		bmProcessByDoc
	End Enum

	'*** Class members
	Private m_oLogin As Login '*** Access to database
	Private m_oRuntimeSession As IRuntimeSession	'*** Access to batches
	Public Event BatchAvailable()	'*** BatchNotification event
	Public m_bEventRemoved As Boolean	'*** Event remove switch
	Private m_oActiveBatch As IBatch	'*** Selected batch object
	Private m_strUniqueId As String	'*** Unique ID for this queue
	Private m_lProcessID As Integer	'*** Process ID for this queue
	Private m_strXmlRtExp As String	'*** XML Runtime Export file
	Private m_strXmlRtImp As String	'*** XML Runtime Import file
	Private m_strXmlSuExp As String	'*** XML Setup Export file
	Private m_strXmlSuImp As String	'*** XML Setup Import file
	Private m_strXmlDocExp As String '*** XML Document Export file
	Private m_strXmlDocImp As String '*** XML Document Export file
	Private m_strXmlPath As String '*** Path for XML files
	Private m_bmOpenMode As bmProcessMode '*** specifies export/import mode
	Private m_bDocOpen As Boolean '*** true if docs have been exported
	Private m_iFirstDoc As Short '*** start of the range of open documents
	Private m_iLastDoc As Short	'*** end of the range of documents
	Public Event OnSessionTimeOut As TimeoutEventHandler(Of TimeoutEventArg) '*** Optional, show custom warning message when timeout

	'*** Constants
	Private Const absAscentRegistryLocation As String = _
	  "Software\Kofax Image Products\Ascent Capture\3.0"

	'**************************************************************************
	'*** Purpose:   Construction with a Unique identifier 
	'***            that specifies the queue
	'*** Inputs:    The Unique ID for the custom module
	'**************************************************************************
	Public Sub New(ByVal strUniqueID As String)
		m_strUniqueId = strUniqueID
	End Sub

	Public Sub LogError( _
	 ByVal lErr1 As Integer, _
	 ByVal strSrc As String, _
	 ByVal lErl As Integer, _
	 ByVal strErrDesc As String, _
	 Optional ByVal bRaise As Boolean = False)

		m_oLogin.LogError(lErr1, 0, 0, strSrc, lErl, strErrDesc, bRaise)
	End Sub

	'**************************************************************************
	'*** Sub:       LoginToRuntimeSession
	'*** Purpose:   Create a runtime session for this application
	'*** Outputs:   None, errors are thrown
	'**************************************************************************
	Public Sub LoginToRuntimeSession()
		'*** True until the event is added
		m_bEventRemoved = True

		'*** Initialize the login object
		If m_oLogin Is Nothing Then
			m_oLogin = New Login

			'*** Enable SecurityBoost within this application.  Setting 
			'*** this property to True only enables SecurityBoost if it is 
			'*** enabled for the entire system - otherwise, it does 
			'*** nothing.  This property can only be modified prior to 
			'*** logging in.
			m_oLogin.EnableSecurityBoost = True

			'*** Gets the name of the Executable
			Dim strPathName As String = Windows.Forms.Application.ExecutablePath
			strPathName = Right(strPathName, 12)
			strPathName = Left(strPathName, 8)

			m_oLogin.Login("", "")
			m_oLogin.ApplicationName = strPathName


			m_oLogin.Version = _
			  FileVersionInfo.GetVersionInfo( _
			  System.Reflection.Assembly.GetExecutingAssembly.Location _
			  ).FileMajorPart & "." _
			  & FileVersionInfo.GetVersionInfo( _
			  System.Reflection.Assembly.GetExecutingAssembly.Location _
			  ).FileMinorPart
		End If

		'*** Validate the user with the module UniqueID
		m_oLogin.ValidateUser(UniqueID)

		m_oRuntimeSession = DirectCast(m_oLogin.RuntimeSession, Kofax.Capture.DBLite.RuntimeSession)

		'*** Add the BatchNotification event handler
		AddHandler m_oRuntimeSession.BatchAvailable, _
		 AddressOf m_oRuntimeSession_BatchAvailable
		m_bEventRemoved = False

		'*** Get the Process ID
		m_lProcessID = m_oLogin.ProcessID

		'*** Assign address of timeout handling routine
		AddHandler m_oRuntimeSession.OnSessionTimeOut, AddressOf ProcessSessionTimeOut
	End Sub

	'**************************************************************************
	'*** Property:  UniqueID
	'*** Purpose:   Unique identifier that specifies the queue.
	'**************************************************************************
	ReadOnly Property UniqueID() As String
		Get
			Return m_strUniqueId
		End Get
	End Property

	'**************************************************************************
	'*** Property:  XmlRuntimeExportFile
	'*** Purpose:   XML file of batch data created when batch is opened.
	'*** Notes:     If a relative file name is passed, a path in the
	'***            Ascent Local directory will be used.
	'**************************************************************************
	Public Property XmlRuntimeExportFile() As String
		Get
			Return m_strXmlRtExp
		End Get
		Set(ByVal Value As String)
			If Len(Value) > 0 And InStr(Value, "\") = 0 Then
				Value = xml_Path() & Value
			End If
			m_strXmlRtExp = Value
		End Set
	End Property

	'**************************************************************************
	'*** Property:  XmlRuntimeImportFile
	'*** Purpose:   XML file of batch data used to populate the database when
	'***            batch is closed.
	'**************************************************************************
	Public Property XmlRuntimeImportFile() As String
		Get
			Return m_strXmlRtImp
		End Get
		Set(ByVal Value As String)
			If Len(Value) > 0 And InStr(Value, "\") = 0 Then
				Value = xml_Path() & Value
			End If
			m_strXmlRtImp = Value
		End Set
	End Property

	'**************************************************************************
	'*** Property:  XmlSetupExportFile
	'*** Purpose:   XML file of batch class data created when batch is opened.
	'*** Notes:     No setup data is exported if this value is blank.
	'**************************************************************************
	Public Property XmlSetupExportFile() As String
		Get
			Return m_strXmlSuExp
		End Get
		Set(ByVal Value As String)
			If Len(Value) > 0 And InStr(Value, "\") = 0 Then
				Value = xml_Path() & Value
			End If
			m_strXmlSuExp = Value
		End Set
	End Property

	'**************************************************************************
	'*** Property:  XmlSetupImportFile
	'*** Purpose:   XML file of batch class data used to populate the database
	'***            when batch is closed.
	'*** Notes:     No setup data is imported if this value is blank.
	'**************************************************************************
	Public Property XmlSetupImportFile() As String
		Get
			Return m_strXmlSuImp
		End Get
		Set(ByVal Value As String)
			If Len(Value) > 0 And InStr(Value, "\") = 0 Then
				Value = xml_Path() & Value
			End If
			m_strXmlSuImp = Value
		End Set
	End Property

	'**************************************************************************
	'*** Property:  XmlDocumentExportFile
	'*** Purpose:   XML file of document data created when docs are opened.
	'*** Notes:     If a relative file name is passed, a path in the
	'***            Ascent Local directory will be used.
	'**************************************************************************
	Public Property XmlDocumentExportFile() As String
		Get
			Return m_strXmlDocExp
		End Get
		Set(ByVal Value As String)
			If Len(Value) > 0 And InStr(Value, "\") = 0 Then
				Value = xml_Path() & Value
			End If
			m_strXmlDocExp = Value
		End Set
	End Property

	'**************************************************************************
	'*** Property:  XmlDocumentImportFile
	'*** Purpose:   XML file of document data used to populate database when
	'***            the documents are closed.
	'**************************************************************************
	Public Property XmlDocumentImportFile() As String
		Get
			Return m_strXmlDocImp
		End Get
		Set(ByVal Value As String)
			If Len(Value) > 0 And InStr(Value, "\") = 0 Then
				Value = xml_Path() & Value
			End If
			m_strXmlDocImp = Value
		End Set
	End Property

	'**************************************************************************
	'*** Property:  ActiveBatch
	'*** Purpose:   Returns currently selected batch
	'**************************************************************************
	Public ReadOnly Property ActiveBatch() As Kofax.Capture.SDK.CustomModule.IBatch
		Get
			Return m_oActiveBatch
		End Get
	End Property

	'**************************************************************************
	'*** Property:  DocumentCount
	'*** Purpose:   Number of documents in the currently selected batch
	'**************************************************************************
	Public ReadOnly Property DocumentCount() As Integer
		Get
			If m_oActiveBatch Is Nothing Then
				Return 0
			Else
				Return m_oActiveBatch.DocumentCount
			End If
		End Get
	End Property

	'**************************************************************************
	'*** Property:  IsDocumentOpen
	'*** Purpose:   Returns True if docs are currently exported
	'**************************************************************************
	Public ReadOnly Property IsDocumentOpen() As Boolean
		Get
			Return m_bDocOpen
		End Get
	End Property

	'**************************************************************************
	'*** Function:  CustomStorageString
	'*** Purpose:   Retrieves the named storage string from the specified batch
	'***            the batch is not opened
	'*** Inputs:    lBatchId - the batch of the string
	'***            strName - the name of the string
	'*** Outputs:   None.
	'**************************************************************************
	Public ReadOnly Property CustomStorageString(ByVal lBatchId As Integer, _
	ByVal strName As String) As String
		Get
			Dim oBatchColl As Kofax.Capture.SDK.CustomModule.IBatchCollection = Nothing
			Dim oBatch As Kofax.Capture.SDK.CustomModule.IBatch = Nothing
			Try
				Try
					'*** Get the collection of all batches for this queue.
					oBatchColl = m_oRuntimeSession.BatchCollection( _
					  KfxDbFilter.KfxDbFilterOnProcess, _
					  m_lProcessID, 0)

					'*** Iterate through the batch collection to find the 
					'*** requested(batch)
					For Each oBatch In oBatchColl
						If oBatch.ExternalBatchID = lBatchId Then
							Exit For
						End If
					Next oBatch

				Catch ex As KfxException
					m_oLogin.LogError(ex.ErrorCode(), 0, 0, THIS_FILE & _
					  ".CustomStorageString", 0, ex.Message, True)
					Throw
				End Try

				'*** Disable local error handling as errors are "expected" here
				If Not IsNothing(oBatch) Then
					Return oBatch.CustomStorageString(strName)
				Else
					Return Nothing
				End If
			Finally
				If Not oBatch Is Nothing Then
					Using (oBatch)
					End Using
					oBatch = Nothing
				End If
				If Not oBatchColl Is Nothing Then
					Using (oBatchColl)
					End Using
					oBatchColl = Nothing
				End If
			End Try
		End Get
	End Property

	'**************************************************************************
	'*** Function:  xml_Path
	'*** Purpose:   Determines the path where XML files are created
	'*** Inputs:    None
	'*** Outputs:   Path where XML files are created
	'**************************************************************************
	Private ReadOnly Property xml_Path() As String
		Get

			'*** If the path is already determined, return it.
			If Len(m_strXmlPath) > 0 Then
				Return m_strXmlPath
				Exit Property
			End If

			'*** Read the registry to determine the local path
			Dim strLocalPath As String
			Dim oACRegKey As RegistryKey
			Dim bNotWritable As Boolean = False
			oACRegKey = Registry.LocalMachine.OpenSubKey(absAscentRegistryLocation, bNotWritable)
			Try
				strLocalPath = oACRegKey.GetValue("LocalPath").ToString()
			Finally
				oACRegKey.Close()
			End Try

			'*** Append the queue ID in order to create a unique location
			'*** Be sure the UniqueID is set first
			Debug.Assert(Len(UniqueID) > 0, "")
			strLocalPath = Path.Combine(strLocalPath, UniqueID) + _
			   Path.DirectorySeparatorChar

			'*** Create the folder if necessary
			If Not Directory.Exists(strLocalPath) Then
				Directory.CreateDirectory(strLocalPath)
			End If
			m_strXmlPath = strLocalPath

			Return m_strXmlPath
		End Get
	End Property

	'**************************************************************************
	'*** Function:  BatchOpen
	'*** Purpose:   Lock the specified batch.
	'*** Inputs:    lBatchId - ID of the batch to open.
	'*** Outputs:   Returns false if no batches are available
	'**************************************************************************
	Public Function BatchOpen(ByVal lBatchId As Integer, Optional _
	ByVal OpenMode As bmProcessMode = _
	bmProcessMode.bmProcessByBatch) As Boolean
		'*** Return value
		Dim bRet As Boolean

		Try
			'*** Check to see if a batch is already selected
			Debug.Assert(m_oActiveBatch Is Nothing, "")

			'*** Be sure the runtime session has been set.
			'*** Set the UniqueID
			Debug.Assert(Not m_oRuntimeSession Is Nothing, "")

			'*** Set import/export mode
			m_bmOpenMode = OpenMode

			'*** Get the collection of all batches for this queue.
			Using oBatchColl As IBatchCollection = m_oRuntimeSession.BatchCollection(KfxDbFilter.KfxDbFilterOnProcess, m_lProcessID, 0)

				'*** Iterate through the batch collection to find the
				'*** requested batch
				For Each oBatch As Kofax.Capture.SDK.CustomModule.IBatch In oBatchColl
					If oBatch.ExternalBatchID = lBatchId Then
						m_oActiveBatch = oBatch
						Exit For
					End If
				Next oBatch
				'*** Return false if no batches are available
				If m_oActiveBatch Is Nothing Then
					bRet = False
				Else
					'*** Lock the batch
					batch_Lock()
					bRet = True
				End If
			End Using
			BatchOpen = bRet
		Catch ex As KfxException
			m_oLogin.LogError(Err.Number, 0, 0, THIS_FILE & ".BatchOpen", 0, _
			  Err.Description, True)
		End Try
	End Function

	'**************************************************************************
	'*** Function:  BatchOpenNext
	'*** Purpose:   Lock the next available batch.
	'*** Inputs:    OpenMode bmProcessByBatch by default
	'***            bExportXML = True by default to export XML
	'*** Outputs:   Returns false if no batches are available
	'**************************************************************************
	Public Function BatchOpenNext( _
	 Optional ByVal OpenMode As bmProcessMode = bmProcessMode.bmProcessByBatch, _
	 Optional ByVal bExportXML As Boolean = True) As Boolean

		Try

			'*** Check to see if a batch is already selected
            'Debug.Assert(m_oActiveBatch Is Nothing, "")

			'*** Set import/export mode
			m_bmOpenMode = OpenMode

			'*** Get the next available batch for this queue
400:		m_oActiveBatch = m_oRuntimeSession.NextBatchGet(m_lProcessID, _
		   KfxDbFilter.KfxDbFilterOnProcess _
		   Or KfxDbFilter.KfxDbFilterOnStates _
		   Or KfxDbFilter.KfxDbSortOnPriorityDescending, _
		   KfxDbState.KfxDbBatchReady _
		   Or KfxDbState.KfxDbBatchSuspended)

			If m_oActiveBatch Is Nothing Then

				'*** No batch available. Return False.
				Return False
			Else
				If bExportXML Then
					'*** Export the XML
					batch_Export()
				End If
				Return True
			End If
		Catch
			m_oLogin.LogError(Err.Number, 0, 0, THIS_FILE & ".BatchOpenNext", _
			 Erl(), Err.Description, True)
		End Try
	End Function

	

	'**************************************************************************
	'*** Function:  BatchClose
	'*** Purpose:   Allows user to close a batch. If closed in the 
	'***            KfxDbBatchReady state it will go to the next module.
	'*** Inputs:    Defaults to Import XML, close in ready state with no errors.
	'***            Optional parameters allow these options to be reset.
	'*** Outputs:   None.
	'**************************************************************************
	Public Sub BatchClose(Optional ByVal eNewState As KfxDbState _
	= KfxDbState.KfxDbBatchReady, Optional ByVal lException As Integer = 0, _
	Optional ByVal strException As String = "", _
	Optional ByVal bImportXML As Boolean = True)
		Dim eQueue As KfxDbQueue

		Try

			'*** Determine what queue to move to based on state.
			Select Case eNewState
				Case KfxDbState.KfxDbBatchError
					eQueue = KfxDbQueue.KfxDbQueueException
				Case KfxDbState.KfxDbBatchReady
					eQueue = KfxDbQueue.KfxDbQueueNext
				Case KfxDbState.KfxDbBatchCompleted
					eQueue = KfxDbQueue.KfxDbQueueSame
				Case Else
					eQueue = KfxDbQueue.KfxDbQueueSame
			End Select

			'*** Import the XML
			If bImportXML Then
				batch_Import()
			End If

			'*** Close the batch
120:        'm_oActiveBatch.BatchClose(eNewState, eQueue, lException, strException)
            'batch_Clear()
            m_oActiveBatch.BatchDelete()
		Catch
			Dim lError As Integer
			Dim strError As String
			Dim strSource As String
			lError = Err.Number
			strError = Err.Description
			strSource = Err.Source
			batch_Clear()
			m_oLogin.LogError(lError, 0, 0, strSource, Erl(), strError, True)
		End Try
	End Sub
	'**************************************************************************
	'*** Function:  batch_Clear
	'*** Purpose:   Clears the batch member and cleans up the XML files.
	'*** Inputs:    None
	'*** Outputs:   None
	'**************************************************************************
	Private Sub batch_Clear()
		m_oActiveBatch = Nothing

		'*** Clear document as well
		DocumentClear()

		'*** Delete the XML files
		file_Delete(XmlSetupExportFile)
		file_Delete(XmlSetupImportFile)
		file_Delete(XmlRuntimeExportFile)
		file_Delete(XmlRuntimeImportFile)
	End Sub

	'**************************************************************************
	'*** Function:  batch_Export
	'*** Purpose:   Exports the XML
	'*** Inputs:    m_oActiveBatch class member.
	'*** Outputs:   Batch database
	'**************************************************************************
	Private Sub batch_Export()
		'*** Export the XML
		Select Case m_bmOpenMode
			Case bmProcessMode.bmProcessByBatch
				m_oActiveBatch.XMLExport(XmlRuntimeExportFile, _
				 XmlSetupExportFile)
			Case bmProcessMode.bmProcessByDoc
				m_oActiveBatch.XMLExportBatchOnly(XmlRuntimeExportFile, _
				 XmlSetupExportFile)
		End Select

		m_bDocOpen = False
	End Sub

	'**************************************************************************
	'*** Function:  batch_Import
	'*** Purpose:   Imports the XML and checks the validity
	'*** Inputs:    m_oActiveBatch class member.
	'*** Outputs:   Batch database
	'**************************************************************************
	Private Sub batch_Import()
		Try
			'*** Do not import if the Setup XML does not validate
			validate_Batch_Setup_Import_Xml()

			'*** Import the XML
			Select Case m_bmOpenMode
				Case bmProcessMode.bmProcessByBatch
					If Dir(XmlRuntimeImportFile) <> "" Then
130:					m_oActiveBatch.XMLImport(XmlRuntimeImportFile)
					End If
				Case bmProcessMode.bmProcessByDoc
					If IsDocumentOpen And Dir(XmlRuntimeImportFile) <> "" Then
135:					m_oActiveBatch.XMLImportBatchOnly(XmlRuntimeImportFile)
					End If
			End Select

			'*** Check the validity of the batch
140:		m_oActiveBatch.ValidityCheck()
		Catch

			'*** Log the error, but don't throw it yet
			Dim lError As Integer
			Dim strError As String
			Dim strSource As String
			lError = Err.Number
			strError = Err.Description
			strSource = Err.Source
			m_oLogin.LogError(lError, 0, 0, THIS_FILE & ".import_Batch", Erl(), _
			 strError, False)

			'*** Rollback the batch
			batch_Rollback(lError, strError)

			'*** Error was already logged, just re-raise it.
			Throw
		End Try
	End Sub

	'**************************************************************************
	'*** Function:  batch_Lock
	'*** Purpose:   Locks the selected batch and exports the XML.
	'*** Inputs:    m_oActiveBatch class member.
	'*** Outputs:   XML file(s)
	'**************************************************************************
	Private Sub batch_Lock()
		'*** Assert state of member variables
		Debug.Assert(Not m_oActiveBatch Is Nothing, "")
		Debug.Assert(m_lProcessID > 0, "")

		'*** Lock the batch
200:	m_oActiveBatch.BatchOpen(m_lProcessID)

		'*** Export the XML
		batch_Export()
	End Sub

	'**************************************************************************
	'*** Function:  batch_Rollback
	'*** Purpose:   Unlocks the batch but abandons any changes to the database.
	'*** Inputs:    m_oActiveBatch class member.
	'*** Outputs:   None
	'**************************************************************************
	Private Sub batch_Rollback(ByVal lError As Integer, _
	ByVal strError As String)
		Try

			'*** The local batch database is invalid at this point.
			'*** Rollback to the original version.
			'*** Any changes made by the queue have been lost.
150:		m_oActiveBatch.BatchClose(KfxDbState.KfxDbBatchSuspended, _
		   KfxDbQueue.KfxDbQueueRollback, lError, strError)
		Catch
			m_oLogin.LogError(Err.Number, 0, 0, THIS_FILE & _
			 ".batch_Rollback", Erl(), Err.Description, False)
		End Try
	End Sub

	'**************************************************************************
	'*** Function:  DocumentOpen
	'*** Purpose:   Open the specified document
	'*** Inputs:    iFirst - start of range, -1 to open all
	'***            iLast  - end of range, omit to open iFirst only
	'*** Outputs:   Returns false if documents cannot be opened
	'**************************************************************************
	Public Sub DocumentOpen(ByVal iFirst As Short, _
	Optional ByVal iLast As Short = -1)
		Try

			'*** Be sure a batch is already open
			Debug.Assert(Not m_oActiveBatch Is Nothing, "")

			'*** Export documents
			If iLast = -1 Then
				m_oActiveBatch.XMLExportDocuments(XmlDocumentExportFile, _
				 iFirst)
			Else
				m_oActiveBatch.XMLExportDocuments(XmlDocumentExportFile, _
				 iFirst, iLast)
			End If

			m_bDocOpen = True
			m_iFirstDoc = iFirst
			m_iLastDoc = iLast
		Catch
			m_oLogin.LogError(Err.Number, 0, 0, THIS_FILE & _
			 ".DocumentOpen", 0, Err.Description, True)
		End Try
	End Sub

	'**************************************************************************
	'*** Function:  DocumentClose
	'*** Purpose:   Close the specified document
	'*** Inputs:    None
	'*** Outputs:   Returns false if documents cannot be opened
	'**************************************************************************
	Public Sub DocumentClose()
		Try

			'*** Be sure a batch is open
			Debug.Assert(Not m_oActiveBatch Is Nothing, "")

			'*** Import documents
			If m_iLastDoc = -1 Then
				m_oActiveBatch.XMLImportDocuments(XmlDocumentImportFile, _
				  m_iFirstDoc)
			Else
				m_oActiveBatch.XMLImportDocuments(XmlDocumentImportFile, _
				  m_iFirstDoc, m_iLastDoc)
			End If
			DocumentClear()
		Catch
			m_oLogin.LogError(Err.Number, 0, 0, THIS_FILE & ".DocumentClose", _
			  Erl(), Err.Description, True)
		End Try
	End Sub


	'**************************************************************************
	'*** Function:  batch_Clear
	'*** Purpose:   Clears the batch member and cleans up the XML files.
	'*** Inputs:    None
	'*** Outputs:   None
	'**************************************************************************
	Private Sub DocumentClear()
		m_bDocOpen = False
		m_iFirstDoc = 0
		m_iLastDoc = 0

		'*** Delete the XML files
		file_Delete(XmlDocumentExportFile)
		file_Delete(XmlDocumentImportFile)
	End Sub
	'**************************************************************************
	'*** Function:  file_Delete
	'*** Purpose:   Deletes a file. Does not error if the file does not exist.
	'*** Inputs:    strFile - File to delete
	'*** Outputs:   None
	'**************************************************************************
	Private Sub file_Delete(ByVal strFile As String)
		Try
			Kill(strFile)
		Catch ex As Exception
		End Try
	End Sub

	'**************************************************************************
	'*** Function:  ReleaseComObjectse
	'*** Purpose:   Releases A.C. objects that might still be open.
	'*** Inputs:    None
	'*** Outputs:   None
	'**************************************************************************
	Public Sub ReleaseComObjects()

		'*** Remove the BatchNotification event handler
		If Not m_bEventRemoved Then
			RemoveHandler m_oRuntimeSession.BatchAvailable, _
			 AddressOf m_oRuntimeSession_BatchAvailable
			RemoveHandler OnSessionTimeOut, AddressOf ProcessSessionTimeOut
			m_bEventRemoved = True
		End If

		If Not IsNothing(m_oRuntimeSession) Then
			Using (m_oRuntimeSession)
			End Using
		End If
		m_oRuntimeSession = Nothing

		'*** Make sure to call Logout to release the Ascent objects when
		'*** shutting down the app
		If Not m_oLogin Is Nothing Then
			m_oLogin.Logout()
		End If
	End Sub

	'**************************************************************************
	'*** Function:  m_oRuntimeSession_BatchAvailable
	'*** Purpose:   Handles and reraises BatchAvailable events from DBLite
	'*** Inputs:    None
	'*** Outputs:   None
	'**************************************************************************
	Private Sub m_oRuntimeSession_BatchAvailable()
		'*** Reraise the event so it can be handled by the calling class
		RaiseEvent BatchAvailable()
	End Sub

	'**************************************************************************
	'*** Property:  ProcessSessionTimeOut
	'*** Inputs:    bShownCustomMsg - return of shown custom waring message or not
	'***				custom module must set bShownCustomMsg to be True if show
	'***				custom warning message
	'*** Purpose:   Raise session timeout message to processing form
	'***			this function use for AutoSignOut feature
	'**************************************************************************
	Public Sub ProcessSessionTimeOut(arg As TimeoutEventArg)
		RaiseEvent OnSessionTimeOut(arg)
	End Sub

	'**************************************************************************
	'*** Property:  AssignHandler
	'*** Purpose:   AutoSignOut feature. Assign handle of form that will 
	'***			handle and process WM_QUERRYENDSESSION and OnSessionTimeOut
	'**************************************************************************
	Public Sub AssignHandler(ByVal hWnd As Integer)
		m_oRuntimeSession.AssignHandler(CType(hWnd, IntPtr))
	End Sub

	'**************************************************************************
	'*** Function:  validate_Batch_Setup_Import_Xml
	'*** Purpose:   Perform XML validation on the Batch Setup XML (if present)
	'*** Inputs:    None
	'*** Outputs:   None
	'**************************************************************************
	Private Sub validate_Batch_Setup_Import_Xml()
		'*** We use the clsAscentXml class, taking advantage of the fact that it
		'*** automatically attempts to open and validate the XML files that we
		'*** assign to it's properties
		Dim oAscentXml As New clsAscentXml
		If (m_strXmlSuImp <> "") Then
			oAscentXml.XmlSetupFile = m_strXmlSuImp
		End If
	End Sub

	'**************************************************************************
	'*** Property:  AvailableBatchCount
	'*** Purpose:   Finds the number of available batches in the custom 
	'***            module's queue
	'*** Inputs:    None
	'*** Outputs:   Number of available batches in the custom module's queue
	'**************************************************************************
	Friend ReadOnly Property AvailableBatchCount() As Integer
		Get
			Dim nAvailableBatches As Integer
			If Not m_oRuntimeSession Is Nothing Then
				nAvailableBatches = m_oRuntimeSession.BatchCollection( _
				 KfxDbFilter.KfxDbFilterOnProcess Or KfxDbFilter.KfxDbFilterOnStates, _
				 m_lProcessID, _
				 KfxDbState.KfxDbBatchReady Or KfxDbState.KfxDbBatchSuspended, _
				 False).Count()
			End If

			'*** Number of ready or suspended batches in the CM's queue
			AvailableBatchCount = nAvailableBatches
		End Get
	End Property
End Class
