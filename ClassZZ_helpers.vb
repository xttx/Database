Public Class ClassZZ_helpers
    Public Shared Function convertToGrayscale(ByVal original As Bitmap) As Bitmap
        Dim newBitmap As New Bitmap(original.Width, original.Height)
        Dim g = Graphics.FromImage(newBitmap)

        Dim m1 As Single() = {0.3F, 0.3F, 0.3F, 0, 0}
        Dim m2 As Single() = {0.59F, 0.59F, 0.59F, 0, 0}
        Dim m3 As Single() = {0.11F, 0.11F, 0.11F, 0, 0}
        Dim m4 As Single() = {0, 0, 0, 1, 0}
        Dim m5 As Single() = {0, 0, 0, 0, 1}
        Dim colorMatrix = New Imaging.ColorMatrix({m1, m2, m3, m4, m5})

        Dim attributes = New Imaging.ImageAttributes
        attributes.SetColorMatrix(colorMatrix)

        g.DrawImage(original, New Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes)
        g.Dispose()
        Return newBitmap
    End Function

End Class
