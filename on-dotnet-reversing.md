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
<br/>

## CLR(공용 언어 런타임)

.NET에서는 CLR이라는 런타임 환경을 제공합니다. 컴파일러와 도구는 CLR의 기능을 노출하며, 관리되는 이 실행 환경을 활용하는 코드를 작성할 수 있게 해줍니다. 런타임을 대상으로 하는, 언어 컴파일러를 사용하여 개발하는 코드를 <strong>관리되는 코드(managed code)</strong> 라고 합니다. 런타임에서 관리되는 코드에 서비스를 제공할 수 있게 하려면 컴파일러에서 사용자 코드의 형식, 멤버 및 참조를 설명하는 메타데이터를 내보내야 합니다. 메타데이터는 코드와 함께 저장되며 로드 가능한 모든 CLR PE 파일에는 메타데이터가 포함되어 있습니다. 런타임에서는 메타데이터를 사용해 클래스를 찾고 로드하며, 메모리에 인스턴스를 배치하고, 메서드 호출을 확인하고, 네이티브 코드를 생성하고, 보안을 강화하며, 런타임 컨텍스트 경계를 설정합니다. 런타임에서는 자동으로 객체 레이아웃을 처리하고 객체에 대한 참조를 관리하며, 이들이 더 이상 사용되지 않을 때는 해제합니다. 수명을 이런식으로 관리하는 객체를 <strong>관리되는 데이터(managed data)</strong>라고 합니다. 가비지 콜렉션을 통해 메모리 누수 뿐만 아니라 기타 몇 가지 일반적인 프로그래밍 오류도 제거됩니다. 코드가 관리되는 경우 .NET 애플리케이션에서 관리되는 데이터와 관리되지 않는 데이터 중 하나 또는 둘 다를 사용할 수 있습니다. 언어 컴파일러에서는 자체 타입(예를 들면 primitive type)을 제공하기 때문에 데이터가 관리되고 있는지 항상 알 수 없으며 알 필요도 없습니다. CLR을 사용하면 컴포넌트 및 애플리케이션에 속한 객체가 여러 언어를 통해 상호작용하는 경우 이를 쉽게 디자인할 수 있습니다.

<br/>

## 관리되는 실행 프로세스
### 1) 컴파일러 선택
사용하는 언어 컴파일러에 따라 사용할 수 있는 런타임 기능이 결정되고 사용자가 해당 기능을 사용해 코드를 디자인합니다. 코드에서 사용해야 하는 구문을 설정하는 것은 런타임이 아니라 컴파일러입니다.
### 2) MSIL로 컴파일
관리되는 코드를 컴파일하는 경우 컴파일러는 소스 코드를 네이티브 코드로 효율적으로 변환할 수 있는 CPU 독립적인 명령 집합인 <strong>MSIL(Microsoft Intermediate Language)</strong>로 변환합니다. 코드를 실행하기 전에 보통 JIT(Just-In-Time) 컴파일러를 통해 MSIL을 CPU 특정적인 코드로 변환해야 합니다. CLR은 지원하는 각 컴퓨터 아키텍처에 대해 JIT 컴파일러를 하나 이상 제공하므로 같은 MSIL 집합이 JIT로 컴파일되고 지원되는 모든 아키텍처에서 실행될 수 있습니다. 컴파일러는 MSIL을 생성할 때 메타데이터도 생성합니다. 메타데이터는 각 타입의 정의, 각 타입 멤버의 시그니처, 코드에서 참조하는 멤버 및 실행 시간에 런타임이 사용하는 기타 데이터를 포함하여 코드의 타입에 대해 설명합니다.
### 3) MSIL을 네이티브 코드로 컴파일
#### (1) JIT 컴파일러에서 컴파일
JIT 컴파일은 어셈블리의 콘텐츠가 로드 및 실행되는 애플리케이션 실행 시간에 요청 시 MSIL을 네이티브 코드로 변환합니다. JIT 컴파일은 일부 코드가 실행 중에 호출되지 않을 가능성을 고려합니다. PE 파일의 모든 MSIL을 네이티브 코드로 변환하는데 시간과 메모리를 사용하는 대신 실행 중에 필요에 따라 MSIL을 변환하고 해당 프로세스의 컨텍스트에서 이후 호출을 위해 액세스할 수 있도록 결과 네이티브 코드를 메모리에 저장합니다. **로더는 타입이 로드되고 초기화될 때 스텁을 만들고 타입 내의 각 메서드에 덧붙입니다.** 메서드가 처음 호출될 때 스텁은 JIT 컴파일러로 제어를 전달하고 JIT 컴파일러는 해당 메서드에 대한 MSIL을 네이티브 코드로 변환하고 생성된 네이티브 코드를 직접 가리키도록 스텁을 수정합니다. 따라서 JIT로 컴파일된 메서드에 대한 이후 호출은 직접 네이티브 코드로 이동합니다.
#### (2) NGen.exe를 사용하여 설치 시간 코드 생성 
JIT 컴파일러에서 생성된 코드는 컴파일을 트리거한 프로세스에 바인딩됩니다. 이 코드는 여러 프로세스에서 공유할 수 없으며, 생성된 코드를 여러 애플리케이션 호출에서 또는 어셈블리 집합을 공유하는 여러 프로세스에서 공유할 수 있도록 CLR은 사전 컴파일 모드를 지원합니다. 이 사전 컴파일 모드에서는 Ngen.exe(네이티브 이미지 생성기)를 사용하여 JIT 컴파일러가 변환하는 것처럼 **MSIL 어셈블리**를 네이티브 코드로 변환합니다.(한 번에 한 메서드가 아니라 한 번에 전체 어셈블리를 컴파일함)

### 4) 코드 실행
메서드를 실행하기 전에 프로세서 특정적인 코드로 컴파일해야 합니다. 실행되는 동안 관리되는 코드는 가비지 콜렉션, 보안, 비관리 코드와의 상호 운용성, 언어 간 디버깅 지원 및 향상된 배포/버전 관리 지원과 같은 서비스를 제공받습니다. Microsoft Windows Vista에서는 운영체제 로더가 COFF 헤더에서 비트를 검사해 관리 모듈을 확인합니다. 설정되는 비트는 관리 모듈을 나타냅니다. 로더는 관리 모듈을 감지하면 mscoree.dll을 로드하고 _CorValidateImage 및 _CorImageUnloading은 관리 모듈 이미지가 로드 및 언로드될 때 로더에 알려줍니다. _CorValidateImage는 우선 코드가 올바른 관리되는 코드인지 확인하고, 이미지의 진입점을 런타임의 진입점으로 변경합니다.
