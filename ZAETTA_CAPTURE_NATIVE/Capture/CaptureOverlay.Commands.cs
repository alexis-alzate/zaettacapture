using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed partial class CaptureOverlay
    {
        private void CopyAndClose()
        {
            if (!HasSelection())
                return;
            CommitTextEdit();
            using (Bitmap result = RenderCrop())
            {
                HistoryService.Save(result);
                ClipboardHelper.SetImageWithRetry((Bitmap)result.Clone());
            }
            Close();
        }

        private void SaveImage()
        {
            if (!HasSelection())
                return;
            CommitTextEdit();
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "PNG (*.png)|*.png";
                dialog.FileName = HistoryService.BuildCaptureFileName();
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    using (Bitmap result = RenderCrop())
                        result.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        private void Undo()
        {
            if (ops.Count == 0)
                return;
            DrawOp removed = ops[ops.Count - 1];
            ops.RemoveAt(ops.Count - 1);
            if (selectedOp == removed)
                selectedOp = null;
            if (movingOp == removed)
                movingOp = null;
            if (resizingOp == removed)
                resizingOp = null;
            resizeHandleIndex = -1;
            Invalidate();
        }
    }
}
