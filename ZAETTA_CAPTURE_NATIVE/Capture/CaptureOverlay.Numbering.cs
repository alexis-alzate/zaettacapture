using System;

namespace ZaettaCaptureNative
{
    internal sealed partial class CaptureOverlay
    {
        private int GetNextNumberValue()
        {
            int max = 0;
            foreach (DrawOp op in ops)
            {
                int value;
                if (op.Tool == Tool.Number && int.TryParse(op.Text, out value))
                    max = Math.Max(max, value);
            }
            return max + 1;
        }

        private void RefreshNextNumberValue()
        {
            counterValue = GetNextNumberValue();
        }
    }
}
