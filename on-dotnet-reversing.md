# .NET 리버싱에 관하여

## .NET 정보를 얻는 방법
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


## CLR(공용 언어 런타임)

.NET에서는 CLR이라는 런타임 환경을 제공합니다. 컴파일러와 도구는 CLR의 기능을 노출하며, 관리되는 이 실행 환경을 활용하는 코드를 작성할 수 있게 해줍니다. 런타임을 대상으로 하는, 언어 컴파일러를 사용하여 개발하는 코드를 **관리되는 코드(managed code)**라고 합니다. 런타임에서 관리되는 코드에 서비스를 제공할 수 있게 하려면 컴파일러에서 사용자 코드의 형식, 멤버 및 참조를 설명하는 메타데이터를 내보내야 합니다. 메타데이터는 코드와 함께 저장되며 로드 가능한 모든 CLR PE 파일에는 메타데이터가 포함되어 있습니다. 런타임에서는 메타데이터를 사용해 클래스를 찾고 로드하며, 메모리에 인스턴스를 배치하고, 메서드 호출을 확인하고, 네이티브 코드를 생성하고, 보안을 강화하며, 런타임 컨텍스트 경계를 설정합니다. 런타임에서는 자동으로 객체 레이아웃을 처리하고 객체에 대한 참조를 관리하며, 이들이 더 이상 사용되지 않을 때는 해제합니다. 수명을 이런식으로 관리하는 객체를 **관리되는 데이터(managed data)**라고 함. 가비지 콜렉션을 통해 메모리 누수 뿐만 아니라 기타 몇 가지 일반적인 프로그래밍 오류도 제거됩니다. 코드가 관리되는 경우 .NET 애플리케이션에서 관리되는 데이터와 관리되지 않는 데이터 중 하나 또는 둘 다를 사용할 수 있습니다. 언어 컴파일러에서는 자체 타입(예를 들면 primitive type)을 제공하기 때문에 데이터가 관리되고 있는지 항상 알 수 없으며 알 필요도 없습니다. CLR을 사용하면 컴포넌트 및 애플리케이션에 속한 객체가 여러 언어를 통해 상호작용하는 경우 이를 쉽게 디자인할 수 있습니다.
