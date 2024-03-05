The project is built in VS 2017 using c# .NET 4.6.1
The External library ICSharpCode.SharpZipLib is included.

The solution includes the library ChartKernel to work with S57 charts encrypted by S63 standard and demo application called "S63". Demo application is based on Windows Form.


How to test:

Enter valid Man_Key  (like 18958)

Enter valid Man_ID (like 335A)

Enter ECDIS unique ID (like unique dongle ID)

Click "Create UserPermit". Check if it is the same as in your ECDIS.


Open file Permit.TXT you received from Chart Provider (like from UKHO)
Select and copy one permit (the whole string can be copied)
Click "Validate Permit" to check if selected permit is valid for this UserPermit and not expired.

Select one encrypted Cell clicking "Browse". The path to the folder with cell should be like ..\ENC_ROOT\US\US410930
NOTE: the decrypted cell will be saved in the same drive in subfolder where the base cell is stored, like ..\ENC_ROOT\US\US410930\2\0\US410930__000. Please check if selected drive is not READONLY.

Click "Decrypt Cell" to get decrypted cell saved in file.