param(
    [Parameter(Mandatory=$true)][string]$SourcePng,
    [Parameter(Mandatory=$true)][string]$OutputIco
)

Add-Type -AssemblyName System.Drawing

$sizes = @(16, 24, 32, 48, 64, 128, 256)
$images = @()
$source = [System.Drawing.Image]::FromFile($SourcePng)

try {
    foreach ($size in $sizes) {
        if ($size -le 32) {
            # Small Windows tray/taskbar icons need the mark itself, not the full app tile.
            $cropSize = [Math]::Min($source.Width, $source.Height) * 0.44
            $cropX = ($source.Width * 0.28)
            $cropY = ($source.Height * 0.27)
        }
        elseif ($size -le 64) {
            $cropSize = [Math]::Min($source.Width, $source.Height) * 0.58
            $cropX = ($source.Width - $cropSize) / 2
            $cropY = ($source.Height - $cropSize) / 2
        }
        else {
            $cropSize = [Math]::Min($source.Width, $source.Height) * 0.68
            $cropX = ($source.Width - $cropSize) / 2
            $cropY = ($source.Height - $cropSize) / 2
        }
        $sourceRect = New-Object System.Drawing.RectangleF $cropX, $cropY, $cropSize, $cropSize

        $bitmap = New-Object System.Drawing.Bitmap $size, $size
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        $memory = New-Object System.IO.MemoryStream
        $targetRect = New-Object System.Drawing.RectangleF 0, 0, $size, $size

        try {
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
            $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $graphics.Clear([System.Drawing.Color]::Transparent)
            $graphics.DrawImage($source, $targetRect, $sourceRect, [System.Drawing.GraphicsUnit]::Pixel)
            $bitmap.Save($memory, [System.Drawing.Imaging.ImageFormat]::Png)
            $images += [pscustomobject]@{
                Size = $size
                Bytes = $memory.ToArray()
            }
        }
        finally {
            $graphics.Dispose()
            $bitmap.Dispose()
            $memory.Dispose()
        }
    }
}
finally {
    $source.Dispose()
}

$stream = [System.IO.File]::Create($OutputIco)
$writer = New-Object System.IO.BinaryWriter($stream)

try {
    $writer.Write([UInt16]0)
    $writer.Write([UInt16]1)
    $writer.Write([UInt16]$images.Count)

    $offset = 6 + (16 * $images.Count)
    foreach ($image in $images) {
        $sizeByte = if ($image.Size -eq 256) { 0 } else { $image.Size }
        $writer.Write([Byte]$sizeByte)
        $writer.Write([Byte]$sizeByte)
        $writer.Write([Byte]0)
        $writer.Write([Byte]0)
        $writer.Write([UInt16]1)
        $writer.Write([UInt16]32)
        $writer.Write([UInt32]$image.Bytes.Length)
        $writer.Write([UInt32]$offset)
        $offset += $image.Bytes.Length
    }

    foreach ($image in $images) {
        $writer.Write($image.Bytes)
    }
}
finally {
    $writer.Close()
    $stream.Close()
}
