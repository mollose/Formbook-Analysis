int[] function(System.Drawing.Bitmap arg)
{
	int[] ret, arr, bytes;
	int offset, width, length, w1, w2;
	System.Drawing.Size size;
	System.Drawing.Color color;
	
	offset = 0;
	size = arg.get_Size();
	width = size.get_Width();
	arr = System.Byte[width * width * 4];
	
	w1 = width - 1;
	for (int i = 0; i <= w1; i++)
	{
		w2 = width - 1;
		for(int j = 0; j <= w2; j++)
		{
			color = arg.GetPixel(9, 0xB);
			System.Buffer::BlockCopy(System.BitConverter::GetBytes(color.ToArgb()), 0, arr, offset, 4);
			offset += 4;
		}
	}
	
	length = System.BitConverter::ToInt32(arr, 0);
	bytes = Byte[length];

	System.Buffer::BlockCopy(arr, 4, bytes, 0, int(bytes.Length));
	ret = bytes;
	return ret;
}