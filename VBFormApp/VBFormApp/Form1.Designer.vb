<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'Clean up any resources being used.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

	'NOTE: The following procedure is required by the Windows Form Designer
	'It can be modified using the Windows Form Designer.  
	'Do not modify it using the code editor.
	<System.Diagnostics.DebuggerStepThrough()>
	Private Sub InitializeComponent()
		txtLog = New TextBox()
		btnInvia = New Button()
		LblStato = New Label()
		SuspendLayout()
		' 
		' txtLog
		' 
		txtLog.BackColor = Color.Black
		txtLog.Font = New Font("Consolas", 10.0F)
		txtLog.ForeColor = Color.Lime
		txtLog.Location = New Point(10, 11)
		txtLog.Multiline = True
		txtLog.Name = "txtLog"
		txtLog.ScrollBars = ScrollBars.Vertical
		txtLog.Size = New Size(540, 300)
		txtLog.TabIndex = 0
		' 
		' btnInvia
		' 
		btnInvia.Location = New Point(10, 319)
		btnInvia.Name = "btnInvia"
		btnInvia.Size = New Size(131, 38)
		btnInvia.TabIndex = 1
		btnInvia.Text = "Invia a VS Code"
		btnInvia.UseVisualStyleBackColor = True
		' 
		' LblStato
		' 
		LblStato.Anchor = AnchorStyles.Top Or AnchorStyles.Right
		LblStato.Location = New Point(507, 319)
		LblStato.Name = "LblStato"
		LblStato.Size = New Size(34, 15)
		LblStato.TabIndex = 2
		LblStato.Text = "Stato"
		LblStato.TextAlign = ContentAlignment.MiddleRight
		' 
		' Form1
		' 
		AutoScaleDimensions = New SizeF(7.0F, 15.0F)
		AutoScaleMode = AutoScaleMode.Font
		ClientSize = New Size(560, 368)
		Controls.Add(LblStato)
		Controls.Add(btnInvia)
		Controls.Add(txtLog)
		Name = "Form1"
		Text = "VB.NET Form"
		ResumeLayout(False)
		PerformLayout()

	End Sub

	Friend WithEvents txtLog As System.Windows.Forms.TextBox
	Friend WithEvents btnInvia As System.Windows.Forms.Button
	Friend WithEvents LblStato As Label

End Class
