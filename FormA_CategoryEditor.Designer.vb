<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormA_CategoryEditor
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
		Me.TreeView1 = New System.Windows.Forms.TreeView()
		Me.TextBox1 = New System.Windows.Forms.TextBox()
		Me.Button3 = New System.Windows.Forms.Button()
		Me.CheckBox1 = New System.Windows.Forms.CheckBox()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.TreeView2 = New System.Windows.Forms.TreeView()
		Me.TextBox2 = New System.Windows.Forms.TextBox()
		Me.Button1 = New System.Windows.Forms.Button()
		Me.Button2 = New System.Windows.Forms.Button()
		Me.SuspendLayout()
		'
		'TreeView1
		'
		Me.TreeView1.HideSelection = False
		Me.TreeView1.Location = New System.Drawing.Point(12, 12)
		Me.TreeView1.Name = "TreeView1"
		Me.TreeView1.Size = New System.Drawing.Size(275, 401)
		Me.TreeView1.TabIndex = 0
		'
		'TextBox1
		'
		Me.TextBox1.Location = New System.Drawing.Point(293, 35)
		Me.TextBox1.Name = "TextBox1"
		Me.TextBox1.Size = New System.Drawing.Size(342, 20)
		Me.TextBox1.TabIndex = 1
		'
		'Button3
		'
		Me.Button3.Location = New System.Drawing.Point(415, 61)
		Me.Button3.Name = "Button3"
		Me.Button3.Size = New System.Drawing.Size(109, 36)
		Me.Button3.TabIndex = 4
		Me.Button3.Text = "Rename"
		Me.Button3.UseVisualStyleBackColor = True
		'
		'CheckBox1
		'
		Me.CheckBox1.AutoSize = True
		Me.CheckBox1.Location = New System.Drawing.Point(293, 12)
		Me.CheckBox1.Name = "CheckBox1"
		Me.CheckBox1.Size = New System.Drawing.Size(231, 17)
		Me.CheckBox1.TabIndex = 5
		Me.CheckBox1.Text = "Show Full Path (to allow moving categories)"
		Me.CheckBox1.UseVisualStyleBackColor = True
		'
		'Label1
		'
		Me.Label1.Location = New System.Drawing.Point(293, 188)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(345, 38)
		Me.Label1.TabIndex = 6
		Me.Label1.Text = "To ADD category, use context menu of category label on the main page"
		'
		'Label2
		'
		Me.Label2.Location = New System.Drawing.Point(293, 226)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(345, 38)
		Me.Label2.TabIndex = 7
		Me.Label2.Text = "To DELETE a category, remove all products from that categorie, and it will be rem" &
	"oved automatically"
		'
		'TreeView2
		'
		Me.TreeView2.HideSelection = False
		Me.TreeView2.Location = New System.Drawing.Point(641, 12)
		Me.TreeView2.Name = "TreeView2"
		Me.TreeView2.Size = New System.Drawing.Size(275, 401)
		Me.TreeView2.TabIndex = 8
		'
		'TextBox2
		'
		Me.TextBox2.Location = New System.Drawing.Point(293, 267)
		Me.TextBox2.Multiline = True
		Me.TextBox2.Name = "TextBox2"
		Me.TextBox2.ReadOnly = True
		Me.TextBox2.Size = New System.Drawing.Size(342, 146)
		Me.TextBox2.TabIndex = 9
		Me.TextBox2.WordWrap = False
		'
		'Button1
		'
		Me.Button1.Location = New System.Drawing.Point(293, 103)
		Me.Button1.Name = "Button1"
		Me.Button1.Size = New System.Drawing.Size(342, 28)
		Me.Button1.TabIndex = 10
		Me.Button1.Text = "Move selected category to the new root ->"
		Me.Button1.UseVisualStyleBackColor = True
		'
		'Button2
		'
		Me.Button2.Location = New System.Drawing.Point(293, 137)
		Me.Button2.Name = "Button2"
		Me.Button2.Size = New System.Drawing.Size(342, 28)
		Me.Button2.TabIndex = 11
		Me.Button2.Text = "Move products from selected category to the new category ->"
		Me.Button2.UseVisualStyleBackColor = True
		'
		'FormA_CategoryEditor
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(927, 425)
		Me.Controls.Add(Me.Button2)
		Me.Controls.Add(Me.Button1)
		Me.Controls.Add(Me.TextBox2)
		Me.Controls.Add(Me.TreeView2)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.Label1)
		Me.Controls.Add(Me.CheckBox1)
		Me.Controls.Add(Me.Button3)
		Me.Controls.Add(Me.TextBox1)
		Me.Controls.Add(Me.TreeView1)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
		Me.Name = "FormA_CategoryEditor"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Category Editor"
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub

	Friend WithEvents TreeView1 As TreeView
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Button3 As Button
    Friend WithEvents CheckBox1 As CheckBox
	Friend WithEvents Label1 As Label
	Friend WithEvents Label2 As Label
	Friend WithEvents TreeView2 As TreeView
	Friend WithEvents TextBox2 As TextBox
	Friend WithEvents Button1 As Button
	Friend WithEvents Button2 As Button
End Class
