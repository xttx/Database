<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form8_pathsCheck
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
		Me.Button1 = New System.Windows.Forms.Button()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.DataGridView1 = New System.Windows.Forms.DataGridView()
		Me.Button2 = New System.Windows.Forms.Button()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.Button3 = New System.Windows.Forms.Button()
		CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'Button1
		'
		Me.Button1.Location = New System.Drawing.Point(12, 95)
		Me.Button1.Name = "Button1"
		Me.Button1.Size = New System.Drawing.Size(207, 55)
		Me.Button1.TabIndex = 0
		Me.Button1.Text = "Check path double in a same game"
		Me.Button1.UseVisualStyleBackColor = True
		'
		'Label1
		'
		Me.Label1.AllowDrop = True
		Me.Label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.Label1.Location = New System.Drawing.Point(12, 208)
		Me.Label1.Name = "Label1"
		Me.Label1.Padding = New System.Windows.Forms.Padding(1)
		Me.Label1.Size = New System.Drawing.Size(207, 55)
		Me.Label1.TabIndex = 2
		Me.Label1.Text = "Drop here a folder, to check its subfolders in db"
		Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		'
		'DataGridView1
		'
		Me.DataGridView1.AllowUserToAddRows = False
		Me.DataGridView1.AllowUserToDeleteRows = False
		Me.DataGridView1.AllowUserToResizeRows = False
		Me.DataGridView1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
			Or System.Windows.Forms.AnchorStyles.Left) _
			Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
		Me.DataGridView1.Location = New System.Drawing.Point(225, 12)
		Me.DataGridView1.Name = "DataGridView1"
		Me.DataGridView1.ReadOnly = True
		Me.DataGridView1.RowHeadersVisible = False
		Me.DataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
		Me.DataGridView1.Size = New System.Drawing.Size(480, 338)
		Me.DataGridView1.TabIndex = 3
		'
		'Button2
		'
		Me.Button2.BackColor = System.Drawing.SystemColors.Control
		Me.Button2.Enabled = False
		Me.Button2.Location = New System.Drawing.Point(12, 156)
		Me.Button2.Name = "Button2"
		Me.Button2.Size = New System.Drawing.Size(207, 23)
		Me.Button2.TabIndex = 4
		Me.Button2.Text = "Clear path double in a same game"
		Me.Button2.UseVisualStyleBackColor = False
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(12, 340)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(40, 13)
		Me.Label2.TabIndex = 5
		Me.Label2.Text = "Rows: "
		'
		'Button3
		'
		Me.Button3.Location = New System.Drawing.Point(12, 12)
		Me.Button3.Name = "Button3"
		Me.Button3.Size = New System.Drawing.Size(207, 55)
		Me.Button3.TabIndex = 6
		Me.Button3.Text = "Check path double"
		Me.Button3.UseVisualStyleBackColor = True
		'
		'Form8_pathsCheck
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(717, 362)
		Me.Controls.Add(Me.Button3)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.Button2)
		Me.Controls.Add(Me.DataGridView1)
		Me.Controls.Add(Me.Label1)
		Me.Controls.Add(Me.Button1)
		Me.Name = "Form8_pathsCheck"
		Me.Text = "Paths Check"
		CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub

	Friend WithEvents Button1 As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents Button2 As Button
	Friend WithEvents Label2 As Label
	Friend WithEvents Button3 As Button
End Class
