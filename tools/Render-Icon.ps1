# Draw the package icon from simple, editable shapes.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path $PSScriptRoot -Parent
$large = [Drawing.Bitmap]::new(1024, 1024)
$small = [Drawing.Bitmap]::new(256, 256)
$graphics = [Drawing.Graphics]::FromImage($large)
$smallGraphics = [Drawing.Graphics]::FromImage($small)
$paper = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#E8D3A8'))
$edge = [Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml('#A98A5D'), 2)
$ink = [Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml('#594631'), 6)
try {
    $graphics.Clear([Drawing.ColorTranslator]::FromHtml('#1C3441'))
    $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.ScaleTransform(4, 4)

    $outline = [Drawing.PointF[]]@(
        [Drawing.PointF]::new(74, 40), [Drawing.PointF]::new(182, 40),
        [Drawing.PointF]::new(182, 207), [Drawing.PointF]::new(168, 199),
        [Drawing.PointF]::new(154, 207), [Drawing.PointF]::new(140, 199),
        [Drawing.PointF]::new(126, 207), [Drawing.PointF]::new(112, 199),
        [Drawing.PointF]::new(98, 207), [Drawing.PointF]::new(84, 199),
        [Drawing.PointF]::new(74, 207)
    )
    $graphics.FillPolygon($paper, $outline)
    $graphics.DrawPolygon($edge, $outline)

    $ink.StartCap = $ink.EndCap = [Drawing.Drawing2D.LineCap]::Round
    $graphics.DrawLine($ink, 94, 80, 158, 80)
    $graphics.DrawLine($ink, 94, 109, 146, 109)
    $graphics.DrawLine($ink, 94, 138, 160, 138)
    $graphics.DrawLine($ink, 94, 167, 138, 167)

    $smallGraphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $smallGraphics.DrawImage($large, [Drawing.Rectangle]::new(0, 0, 256, 256))
    $small.Save((Join-Path $root 'icon.png'), [Drawing.Imaging.ImageFormat]::Png)
}
finally {
    $ink.Dispose()
    $edge.Dispose()
    $paper.Dispose()
    $smallGraphics.Dispose()
    $graphics.Dispose()
    $small.Dispose()
    $large.Dispose()
}
