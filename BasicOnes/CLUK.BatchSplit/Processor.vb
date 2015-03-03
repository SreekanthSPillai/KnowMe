'******************************************************************************
'***
'*** Module:    SplitProcessor.vb - SplitProcessor
'*** Purpose:   Encapsulates the separate process.
'***
'*** (c) Copyright 2009 Kofax Image Products.
'*** All rights reserved.
'***
'******************************************************************************
Option Strict On
Option Explicit On
Option Compare Text
Imports System.Runtime.InteropServices
Imports System.Collections.Generic
Imports Kofax.Capture.SDK.CustomModule
Imports Kofax.Capture.DBLite
Imports CLUK.BatchSplit.Cluk.Xrm.WebService.Models.QuoteManagerDAO

Namespace CLUK.BatchSplit
    Public Class Processor
        Implements IProcessor

        '*** Constants
        Const c_ErrMoveElemnent As Integer = 100

        Private Const c_DOCUMENTS As String = "Documents"
        Private Const c_DOCUMENT As String = "Document"
        Private Const c_BATCH As String = "Batch"
        Private Const c_FORMTYPE As String = "FormTypeName"

        Dim sQuote As String = String.Empty
        Dim sTempQuote As String = String.Empty

        Dim sSurname As String = String.Empty
        Dim sQuoteprefix As String = String.Empty
        Dim sQuoteVersion As String = String.Empty
        Dim sTempQuoteVersion As String = String.Empty

        Dim sQuoteVariation As String = String.Empty
        Dim sTempQuoteVaration As String = String.Empty

        Dim sQuoteNumber As String = String.Empty
        Dim sDOB As String = String.Empty
        Dim sPostCode As String = String.Empty
        Dim sOriginalBatchID As String = String.Empty
        Dim sSource As String = String.Empty
        Dim sTempOriginalBatchID As String = String.Empty
        Dim sTempQuoteNumber As String = String.Empty
        Dim sTempQuotePrefix As String = String.Empty
        Dim sTempSurname As String = String.Empty
        Dim sTempDOB As String = String.Empty
        Dim sTempPostCode As String = String.Empty

        Dim sDocType As String = String.Empty

        Dim sFormType As String = String.Empty

        '----------CanRetire-------------
        Dim sPIPNumber As String = String.Empty
        Dim sTempPIPNumber As String = String.Empty
        Dim sFDPNumber As String = String.Empty
        Dim sTempFDPNumber As String = String.Empty
        Dim sFTINumber As String = String.Empty
        Dim sTempFTINumber As String = String.Empty
        Dim sAnnuityprefix As String = String.Empty
        Dim sTempAnnuityprefix As String = String.Empty
        Dim sAnnuitynumber As String = String.Empty
        Dim sTempAnnuitynumber As String = String.Empty
        Dim sAnnuityversion As String = String.Empty
        Dim sTempAnnuityversion As String = String.Empty
        Dim sAnnuityvariation As String = String.Empty
        Dim sTempAnnuityvariation As String = String.Empty


        '********************************************************************************
        '*** Sub:       PageStatus
        '*** Purpose:   Raise a status string for the Page label
        '********************************************************************************
        Event PageStatus(ByVal strStatus As String) Implements IProcessor.PageStatus

        '********************************************************************************
        '*** Sub:       DocumentStatus
        '*** Purpose:   Raise a status string for the Document label
        '********************************************************************************
        Event DocumentStatus(ByVal strStatus As String) Implements IProcessor.DocumentStatus

        '********************************************************************************
        '*** Sub:       CustomModuleID
        '*** Purpose:   The UniqueID of the custom module
        '********************************************************************************
        ReadOnly Property CustomModuleID() As String Implements IProcessor.CustomModuleID
            Get
                Const CUSTMOD_ID As String = "Kofax.CLUKBatchSplit"
                CustomModuleID = CUSTMOD_ID
            End Get
        End Property

        '********************************************************************************
        '*** Sub:       ProcessDescription
        '*** Purpose:   The feature description of the custom module
        '********************************************************************************
        ReadOnly Property ProcessDescription() As String Implements IProcessor.ProcessDescription
            Get
                Return My.Resources.Feature
            End Get
        End Property

        '********************************************************************************
        '*** Sub:       ProcessCaption
        '*** Purpose:   The caption to be displayed on the form using this process
        '********************************************************************************
        ReadOnly Property ProcessCaption() As String Implements IProcessor.ProcessCaption
            Get
                Return My.Resources.Caption
            End Get
        End Property

        '********************************************************************************
        '*** Sub:       Process
        '*** Purpose:   Splits batch into Valid and Invalid batches
        '*** Inputs:    oBatch - The batch to be processed.
        '*** Outputs:   None
        '********************************************************************************
        Public Sub Process(ByVal oBatch As Kofax.Capture.SDK.CustomModule.IBatch) Implements IProcessor.Process

            '*** Find documents in batch
            Dim oColDocs As Kofax.Capture.SDK.Data.IACDataElementCollection = _
       GetDocsElementCollection(oBatch)


            Dim oDictionaryValidDocs As New Dictionary(Of String, List(Of Kofax.Capture.SDK.Data.IACDataElement))
            Dim oDictionaryInValidDocs As New Dictionary(Of String, List(Of Kofax.Capture.SDK.Data.IACDataElement))

            For Each oDoc As Kofax.Capture.SDK.Data.IACDataElement In oColDocs

                Dim oIndex As Kofax.Capture.SDK.Data.IACDataElement = oDoc.FindChildElementByName("IndexFields")
                Dim oIndexColl As Kofax.Capture.SDK.Data.IACDataElementCollection = oIndex.FindChildElementsByName("IndexField")

                ''''Loop through fields to get the values you require - Quote,Surname,Prefix Etc
                For Each oField As Kofax.Capture.SDK.Data.IACDataElement In oIndexColl
                    If oField("Name").ToString() = "Quotenumber" Then
                        sQuote = oField("Value").ToString()
                    End If

                    If oField("Name").ToString() = "Quoteprefix" Then
                        sQuoteprefix = oField("Value").ToString()
                    End If

                    If oField("Name").ToString() = "QuoteVersion" Then
                        sQuoteVersion = oField("Value").ToString()
                    End If

                    If oField("Name").ToString() = "Quotevariation" Then
                        sQuoteVariation = oField("Value").ToString()
                    End If



                    If oField("Name").ToString() = "Source" Then
                        sSource = oField("Value").ToString()
                    End If

                    '------------ Can Retire -----------------------
                    If oField("Name").ToString() = "PIPNumber" Then
                        sPIPNumber = oField("Value").ToString()
                    End If

                    If oField("Name").ToString() = "FDPNumber" Then
                        sFDPNumber = oField("Value").ToString()
                    End If

                    If oField("Name").ToString() = "FTINumber" Then
                        sFTINumber = oField("Value").ToString()
                    End If

                    If oField("Name").ToString() = "Annuityprefix" Then
                        sAnnuityprefix = oField("Value").ToString()
                    End If

                    If oField("Name").ToString() = "Annuitynumber" Then
                        sAnnuitynumber = oField("Value").ToString()
                    End If

                    If oField("Name").ToString() = "AnnuityVersion" Then
                        sAnnuityversion = oField("Value").ToString()
                    End If

                    If oField("Name").ToString() = "Annuityvariation" Then
                        sAnnuityvariation = oField("Value").ToString()
                    End If

                    If oField("Name").ToString() = "Surname" Then
                        sSurname = oField("Value").ToString()
                    End If

                    If oField("Name").ToString() = "DOB" Then
                        sDOB = oField("Value").ToString()
                    End If

                    If oField("Name").ToString() = "PostCode" Then
                        sPostCode = oField("Value").ToString()
                    End If

                    If oField("Name").ToString() = "Doctype" Then
                        sDocType = oField("Value").ToString()
                    End If

                    If oField("Name").ToString() = "Formtype" Then
                        sFormType = oField("Value").ToString()
                    End If


                Next

                Dim oBatchElement As Kofax.Capture.SDK.Data.IACDataElement = GetBatchElement(oBatch)
                Dim oBatchIndex As Kofax.Capture.SDK.Data.IACDataElement = oBatchElement.FindChildElementByName("BatchFields")
                Dim oBatchIndexIndexColl As Kofax.Capture.SDK.Data.IACDataElementCollection = oBatchIndex.FindChildElementsByName("BatchField")

                ''''Loop through fields to set the BatchValid value
                For Each oField As Kofax.Capture.SDK.Data.IACDataElement In oBatchIndexIndexColl
                    If oField("Name").ToString() = "BatchID" Then

                        sOriginalBatchID = oField("Value")

                    End If


                Next

                '' Tactical Solution to retain sticky Values
                If (sFormType = String.Empty) Then
                    If sQuote = String.Empty Then
                        sQuote = sTempQuote
                        sQuoteprefix = sTempQuotePrefix
                        sSurname = sTempSurname
                        sDOB = sTempDOB
                        sQuoteVariation = sTempQuoteVaration
                        sQuoteVersion = sTempQuoteVersion
                    Else
                        sTempQuote = sQuote
                        sTempQuotePrefix = sQuoteprefix
                        sTempSurname = sSurname
                        sTempDOB = sDOB
                        sTempQuoteVaration = sQuoteVariation
                        sTempQuoteVersion = sQuoteVersion
                    End If
                End If

                '' Tactical Solution to retain sticky Values
                If (sFormType = "CANRETIRE") Then
                    ''If (sAnnuitynumber = String.Empty And sAnnuityprefix = String.Empty And sAnnuityvariation = String.Empty And sAnnuityversion = String.Empty And sPIPNumber = String.Empty And sFTINumber = String.Empty And sFDPNumber = String.Empty) Then

                    sTempAnnuitynumber = sAnnuitynumber
                    sTempAnnuityprefix = sAnnuityprefix
                    sTempAnnuityvariation = sAnnuityvariation
                    sTempAnnuityversion = sAnnuityversion
                    sTempPIPNumber = sPIPNumber
                    sTempFDPNumber = sFDPNumber
                    sTempFTINumber = sFTINumber
                    sTempSurname = sSurname
                    sTempDOB = sDOB
                    sTempPostCode = sPostCode

                ElseIf (sFormType = "SUPPORTDOC") Then
                    sAnnuityprefix = sTempAnnuityprefix
                    sAnnuitynumber = sTempAnnuitynumber
                    sAnnuityvariation = sTempAnnuityvariation
                    sAnnuityversion = sTempAnnuityversion
                    sPIPNumber = sTempPIPNumber
                    sFTINumber = sTempFTINumber
                    sFDPNumber = sTempFDPNumber
                    sSurname = sTempSurname
                    sDOB = sTempDOB
                    sPostCode = sTempPostCode
                End If


                Dim bIsAllQuoteValid As Boolean = False
                Dim bQuoteValid As Boolean = True
                Dim bAnnuityQuoteValid As Boolean = True
                Dim bPIPQuoteValid As Boolean = True
                Dim bFDPQuoteValid As Boolean = True
                Dim bFTPQuoteValid As Boolean = True

                ''''Call Validation here
                If (sQuote <> String.Empty) Then
                    bQuoteValid = FindQuoteinDynamics(sQuoteprefix, sQuote, sQuoteVariation, sQuoteVersion, sSurname)
                End If

                If (sAnnuitynumber <> String.Empty) Then
                    bAnnuityQuoteValid = FindQuoteinDynamics(sAnnuityprefix, sAnnuitynumber, sAnnuityvariation, sAnnuityversion, sSurname)
                End If

                If (sPIPNumber <> String.Empty) Then
                    bPIPQuoteValid = FindQuoteinDynamics("PIP", sPIPNumber, Nothing, Nothing, sSurname)
                End If

                If (sFDPNumber <> String.Empty) Then
                    bFDPQuoteValid = FindQuoteinDynamics("FDP", sFDPNumber, Nothing, Nothing, sSurname)
                End If

                If (sFTINumber <> String.Empty) Then
                    bFTPQuoteValid = FindQuoteinDynamics("FTI", sFTINumber, Nothing, Nothing, sSurname)
                End If

                If (bQuoteValid And bAnnuityQuoteValid And bPIPQuoteValid And bFDPQuoteValid And bFTPQuoteValid) Then
                    bIsAllQuoteValid = True
                End If


                If bIsAllQuoteValid Then
                    ''''Add to good docs collection
                    If Not oDictionaryValidDocs.ContainsKey("Valid") Then
                        oDictionaryValidDocs("Valid") = New List(Of Kofax.Capture.SDK.Data.IACDataElement)
                    End If
                    oDictionaryValidDocs("Valid").Add(oDoc)
                    ''Below code is to sort out sticky field issue in Validation Module
                    For Each oField As Kofax.Capture.SDK.Data.IACDataElement In oIndexColl
                        If oField("Name").ToString() = "Quotenumber" Then
                            oField("Value") = sQuote
                        End If

                        If oField("Name").ToString() = "Surname" Then
                            oField("Value") = sSurname
                        End If

                        If oField("Name").ToString() = "Quoteprefix" Then
                            oField("Value") = sQuoteprefix
                        End If

                        If oField("Name").ToString() = "Quotevariation" Then
                            oField("Value") = sQuoteVariation
                        End If

                        If oField("Name").ToString() = "QuoteVersion" Then
                            oField("Value") = sQuoteVersion
                        End If

                        If oField("Name").ToString() = "DOB" Then
                            oField("Value") = sDOB
                        End If

                        If oField("Name").ToString() = "OriginalBatchID" Then
                            oField("Value") = sOriginalBatchID
                        End If

                        'Surname,DOB and Postcode are common

                        '------------ Can Retire -----------------------
                        If oField("Name").ToString() = "PIPNumber" Then
                            oField("Value") = sPIPNumber
                        End If

                        If oField("Name").ToString() = "FDPNumber" Then
                            oField("Value") = sFDPNumber
                        End If

                        If oField("Name").ToString() = "FTINumber" Then
                            oField("Value") = sFTINumber
                        End If

                        If oField("Name").ToString() = "Annuityprefix" Then
                            oField("Value") = sAnnuityprefix
                        End If

                        If oField("Name").ToString() = "Annuitynumber" Then
                            oField("Value") = sAnnuitynumber
                        End If

                        If oField("Name").ToString() = "AnnuityVersion" Then
                            oField("Value") = sAnnuityversion
                        End If

                        If oField("Name").ToString() = "Annuityvariation" Then
                            oField("Value") = sAnnuityvariation
                        End If

                    Next
                Else
                    ''''Add to bad docs collection
                    If Not oDictionaryInValidDocs.ContainsKey("InValid") Then
                        oDictionaryInValidDocs("InValid") = New List(Of Kofax.Capture.SDK.Data.IACDataElement)
                    End If
                    oDictionaryInValidDocs("InValid").Add(oDoc)
                    For Each oField As Kofax.Capture.SDK.Data.IACDataElement In oIndexColl
                        If oField("Name").ToString() = "OriginalBatchID" Then
                            oField("Value") = sOriginalBatchID
                        End If

                        If oField("Name").ToString() = "PIPNumber" Then
                            oField("Value") = sPIPNumber
                        End If

                        If oField("Name").ToString() = "FDPNumber" Then
                            oField("Value") = sFDPNumber
                        End If

                        If oField("Name").ToString() = "FTINumber" Then
                            oField("Value") = sFTINumber
                        End If

                        If oField("Name").ToString() = "Annuityprefix" Then
                            oField("Value") = sAnnuityprefix
                        End If

                        If oField("Name").ToString() = "Annuitynumber" Then
                            oField("Value") = sAnnuitynumber
                        End If

                        If oField("Name").ToString() = "AnnuityVersion" Then
                            oField("Value") = sAnnuityversion
                        End If

                        If oField("Name").ToString() = "Annuityvariation" Then
                            oField("Value") = sAnnuityvariation
                        End If


                        If oField("Name").ToString() = "Surname" Then
                            oField("Value") = sSurname
                        End If

                        If oField("Name").ToString() = "DOB" Then
                            oField("Value") = sDOB
                        End If

                        If oField("Name").ToString() = "PostCode" Then
                            oField("Value") = sPostCode
                        End If

                    Next
                End If


            Next

            ' '''''Create the Valid Batch

            If Not (oDictionaryValidDocs.Count = 0) Then
                SplitDocuments(oBatch, oDictionaryValidDocs("Valid"), True)
            End If

            ' ''''''Create the InValid batch
            If Not (oDictionaryInValidDocs.Count = 0) Then
                SplitDocuments(oBatch, oDictionaryInValidDocs("InValid"), False)
            End If

            ' '''''Create the Valid Batch
            'For Each sValid As String In oDictionaryValidDocs.Keys
            '    SplitDocuments(oBatch, oDictionaryValidDocs(sValid), True)
            'Next

            ' ''''''Create the InValid batch
            'For Each sInValid As String In oDictionaryInValidDocs.Keys
            '    SplitDocuments(oBatch, oDictionaryInValidDocs(sInValid), False)
            'Next

        End Sub



        Private Function FindQuoteinDynamics(ByVal quotePrefix As String, ByVal quoteNumber As String, ByVal quoteVariation As String, ByVal quoteVersion As String, ByRef surName As String) As Boolean
            Dim bFound As Boolean
            Dim quoteManagerClient As New QuoteManagerServiceClient()
            Dim validQuoteResponse As New ValidQuoteResponse()
            Dim quoteReference As New QuoteReference()
            quoteReference.ProductPrefix = quotePrefix
            quoteReference.Reference = quoteNumber
            quoteReference.Reference = quoteNumber
            quoteReference.AlphaSuffix = quoteVersion
            quoteReference.NumericSuffix = quoteVariation
            validQuoteResponse = quoteManagerClient.IsQuoteValid(quoteReference, surName, Nothing, Nothing)
            bFound = validQuoteResponse.QuoteMatch
            'bFound = True 'Addded for Testing
            Return bFound
        End Function


        '********************************************************************************
        '*** Sub:       SplitDocuments
        '*** Purpose:   Split all document in the collection into a new child batch.
        '*** Inputs:    oBatch -   The parent batch to create child batch for spliting.
        '*** Outputs:   oColDocs - collection of the documents to be 
        '***                       split into child batch.
        '********************************************************************************
        Private Sub SplitDocuments( _
         ByVal oBatch As Kofax.Capture.SDK.CustomModule.IBatch, _
         ByVal oList As List(Of Kofax.Capture.SDK.Data.IACDataElement), _
         ByVal bValid As Boolean)

            If oList IsNot Nothing AndAlso oList.Count > 0 Then

                '*** Create the child batch
                Dim oChildBatch As Kofax.Capture.SDK.CustomModule.IBatch = oBatch.ChildBatchCreate()

                '*** Update the Batch Index field to Y or N depending on it's validitity
                UpDateBatchValidField(oChildBatch, bValid)

                Try
                    '*** Get the child's Documents element
                    Dim oChildDocsElement As Kofax.Capture.SDK.Data.IACDataElement = GetDocumentsElement(oChildBatch)

                    For Each oDocElement As Kofax.Capture.SDK.Data.IACDataElement In oList

                        '*** Split this current document into the child batch
                        oChildDocsElement.MoveElementToBatch(oDocElement)
                        RaiseEvent DocumentStatus( _
                         String.Format(My.Resources.DocSplitStatus, ""))

                        oDocElement = Nothing
                    Next

                    RaiseEvent DocumentStatus( _
                      String.Format(My.Resources.DocsStatus, _
                       oList.Count, "", oChildBatch.Name))

                    oChildBatch.BatchClose(KfxDbState.KfxDbBatchReady, _
                      KfxDbQueue.KfxDbQueueNext, 0, "")


                Catch ex As COMException
                    If Not IsNetworkError(ex) Then
                        '*** Try to close the batch to the exception queue
                        oChildBatch.BatchClose( _
                         KfxDbState.KfxDbBatchError, _
                         KfxDbQueue.KfxDbQueueException, _
                         c_ErrMoveElemnent, ex.Message)
                    Else
                        Throw ex
                    End If

                End Try
                oChildBatch = Nothing
            End If

        End Sub
        Private Sub UpDateBatchValidField(ByVal oBatch As Kofax.Capture.SDK.CustomModule.IBatch, ByVal bValidBatch As Boolean)

            Dim oBatchElement As Kofax.Capture.SDK.Data.IACDataElement = GetBatchElement(oBatch)
            Dim oBatchIndex As Kofax.Capture.SDK.Data.IACDataElement = oBatchElement.FindChildElementByName("BatchFields")
            Dim oBatchIndexIndexColl As Kofax.Capture.SDK.Data.IACDataElementCollection = oBatchIndex.FindChildElementsByName("BatchField")

            ''''Loop through fields to set the BatchValid value
            For Each oField As Kofax.Capture.SDK.Data.IACDataElement In oBatchIndexIndexColl
                If oField("Name").ToString() = "BatchValid" Then
                    If bValidBatch Then
                        oField("Value") = "Y"
                    End If
                Else
                    oField("Value") = "N"
                End If


            Next

        End Sub

        '**************************************************************************************
        '***** Methods to access the various ACDataelement of the batch
        '**************************************************************************************
        Protected Function GetBatchElement(ByVal oBatch As Kofax.Capture.SDK.CustomModule.IBatch) _
        As Kofax.Capture.SDK.Data.IACDataElement
            '*** Access Runtime Information - start from the root node
            Dim oRootElement As Kofax.Capture.SDK.Data.IACDataElement = _
       oBatch.ExtractRuntimeACDataElement(0)
            '*** Batch node
            Dim oBatchElement As Kofax.Capture.SDK.Data.IACDataElement = _
       oRootElement.FindChildElementByName(c_BATCH)

            Return oBatchElement
        End Function

        Protected Function GetDocumentsElement(ByVal oBatch As Kofax.Capture.SDK.CustomModule.IBatch) _
     As Kofax.Capture.SDK.Data.IACDataElement
            '*** Batch node
            Dim oBatchElement As Kofax.Capture.SDK.Data.IACDataElement = _
             GetBatchElement(oBatch)
            '*** Documents node
            Dim oDocumentsElement As Kofax.Capture.SDK.Data.IACDataElement = _
             oBatchElement.FindChildElementByName(c_DOCUMENTS)

            Return oDocumentsElement
        End Function

        Protected Function GetDocsElementCollection(ByVal oBatch As Kofax.Capture.SDK.CustomModule.IBatch) _
     As Kofax.Capture.SDK.Data.IACDataElementCollection
            '*** Documents node
            Dim oDocsElement As Kofax.Capture.SDK.Data.IACDataElement = GetDocumentsElement(oBatch)
            '*** Find documents in batch
            Dim oColDocs As Kofax.Capture.SDK.Data.IACDataElementCollection = _
             oDocsElement.FindChildElementsByName(c_DOCUMENT)

            Return oColDocs

        End Function

        Public Shared Function IsNetworkError(ByVal ex As COMException) As Boolean

            Const c_nComErrorMask As Integer = 65535
            Const c_nKfxNetworkError As Integer = 3566

            Dim nError As Integer = ex.ErrorCode And c_nComErrorMask
            Return nError = c_nKfxNetworkError

        End Function

        Private Function IsQuoteValid() As Boolean
            Return True
        End Function
    End Class
End Namespace
