%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe ProcessWindow.exe
Net Start ProcessWindow
sc config ProcessWindow start= demand