# Render the receipt symbol at 4x, then downsample to a Thunderstore icon.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path $PSScriptRoot -Parent
$large = [Drawing.Bitmap]::new(1024, 1024)
$small = [Drawing.Bitmap]::new(256, 256)
$graphics = [Drawing.Graphics]::FromImage($large)
$smallGraphics = [Drawing.Graphics]::FromImage($small)
$paper = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#F4F7F9'))
$ink = [Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml('#29465B'), 6)
$accent = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#E9AA52'))
$accentInk = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#122433'))
try {
    $graphics.Clear([Drawing.ColorTranslator]::FromHtml('#122433'))
    $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.ScaleTransform(4, 4)

    $outline = [Drawing.PointF[]]@(
        [Drawing.PointF]::new(72, 39), [Drawing.PointF]::new(184, 39),
        [Drawing.PointF]::new(184, 210), [Drawing.PointF]::new(170, 200),
        [Drawing.PointF]::new(156, 210), [Drawing.PointF]::new(142, 200),
        [Drawing.PointF]::new(128, 210), [Drawing.PointF]::new(114, 200),
        [Drawing.PointF]::new(100, 210), [Drawing.PointF]::new(86, 200),
        [Drawing.PointF]::new(72, 210)
    )
    $graphics.FillPolygon($paper, $outline)
    $graphics.FillRectangle($accent, 91, 65, 74, 10)
    foreach ($y in @(100, 122, 144)) {
        $graphics.DrawLine($ink, 91, $y, 147, $y)
    }
    $graphics.FillEllipse($accent, 143, 143, 52, 52)
    $graphics.FillRectangle($accentInk, 165, 152, 8, 21)
    $graphics.FillEllipse($accentInk, 165, 178, 8, 8)

    $smallGraphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $smallGraphics.DrawImage($large, [Drawing.Rectangle]::new(0, 0, 256, 256))
    $small.Save((Join-Path $root 'icon.png'), [Drawing.Imaging.ImageFormat]::Png)
}
finally {
    $accentInk.Dispose()
    $accent.Dispose()
    $ink.Dispose()
    $paper.Dispose()
    $smallGraphics.Dispose()
    $graphics.Dispose()
    $small.Dispose()
    $large.Dispose()
}
