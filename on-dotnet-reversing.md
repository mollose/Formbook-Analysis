# .NET 리버싱에 관하여

### .NET 정보를 얻는 방법
PE 파일 형식에 담긴 .NET 정보는 최소한이며, .NET 실행파일의 목적은 메타데이터와 IL(Intermediate Language) 정보를 운영체제로부터 얻어내는 것입니다. .NET 파일은 결국 MSCOREE.DLL에 연결되어 있고, 이 DLL은 .NET 실행파일의 엔트리 포인트가 됩니다. .NET 실행파일이 실행될 때 엔트리 포인트는 아주 작은 스텁 코드이며, 이 스텁 코드는 MSCOREE.DLL 파일 안에 있는 함수로 바로 이동하도록 되어 있는데 이는 _CorExeMain이거나 _CorDllMain입니다. 그러면 MSCOREE.DLL은 메타데이터와 IL을 이용해 파일을 실행합니다. .NET에 관한 정보는 IMAGE_COR20_HEADER 구조체에 담겨 있습니다.

```
typedef struct _IMAGE_COR20_HEADER
{    
  // Header versioning
  ULONG cb;
  USHORT MajorRuntimeVersion;
  USHORT MinorRuntimeVersion;
  //Symbol table and startup information
  IMAGE_DATA_DIRECTORY MetaData;
  ULONG Flags;
  ULONG EntryPointToken;
  // Binding information    
  IMAGE_DATA_DIRECTORY Resources;
  IMAGE_DATA_DIRECTORY StrongNameSignature;
  //Regular fixup and binding information
  IMAGE_DATA_DIRECTORY CodeManagerTable;
  IMAGE_DATA_DIRECTORY VTableFixups;
  IMAGE_DATA_DIRECTORY ExportAddressTableJumps;
  // Precompiled image info (internal use only – set to zero)    
  IMAGE_DATA_DIRECTORY ManagedNativeHeader;
} IMAGE_COR20_HEADER;
```
