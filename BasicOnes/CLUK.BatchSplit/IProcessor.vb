'******************************************************************************
'***
'*** Module:    IProcessor.vb - IProcessor
'*** Purpose:   Interface that implements the batch processing.
'***
'*** (c) Copyright 2009 Kofax Image Products.
'*** All rights reserved.
'***
'******************************************************************************
Namespace CLUK.BatchSplit
    Public Interface IProcessor

        '********************************************************************************
        '*** Sub:       PageStatus
        '*** Purpose:   Raise a status string for the Page label
        '********************************************************************************
        Event PageStatus(ByVal strStatus As String)

        '********************************************************************************
        '*** Sub:       DocumentStatus
        '*** Purpose:   Raise a status string for the Document label
        '********************************************************************************
        Event DocumentStatus(ByVal strStatus As String)

        '********************************************************************************
        '*** Sub:       CustomModuleID
        '*** Purpose:   The UniqueID of the custom module
        '********************************************************************************
        ReadOnly Property CustomModuleID() As String

        '********************************************************************************
        '*** Sub:       ProcessDescription
        '*** Purpose:   The feature description of the custom module
        '********************************************************************************
        ReadOnly Property ProcessDescription() As String

        '********************************************************************************
        '*** Sub:       ProcessCaption
        '*** Purpose:   The caption to be displayed on the form using this process
        '********************************************************************************
        ReadOnly Property ProcessCaption() As String

        '********************************************************************************
        '*** Sub:       Process
        '*** Purpose:   Process the batch.
        '*** Inputs:    oBatch - The batch to be processed.
        '*** Outputs:   None
        '********************************************************************************
        Sub Process(ByVal oBatch As Kofax.Capture.SDK.CustomModule.IBatch)

    End Interface
End Namespace
