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


</br>

## 관리되는 컴파일
관리되는 컴파일러는 코드(*.cs 파일)를 CIL(Common Intermediate Language. 국제 표준에서 사용되는 용어이며, 이러한 표준의 마이크로소프트 구현체를 가리키는 제품명이 MSIL) 코드, 매니페스트, 그리고 메타데이터로 번역합니다. 이 과정은 보통 두 컴파일 단계를 거칩니다. 첫 번째 컴파일 단계는 컴파일러에 의해 수행되며 소스 코드가 MSIL로 변환됩니다. 두 번째 컴파일 단계는 실행 시간에 수행되며 여기서 MSIL 코드가 네이티브 코드로 컴파일됩니다. 마지막으로, CIL은 완전히 발달된 .NET 프로그래밍 언어이며, 자신의 구문과 컴파일러를 별도로 가집니다.

</br>

## CIL의 이해
CIL은 그저 또 다른 구조적인 .NET 프로그래밍 언어이며, CIL과, .NET 프레임워크에 내장된 CIL 컴파일러(ILASM.EXE)를 사용해 직접 .NET 어셈블리를 빌드하는 것도 가능합니다. CIL에 대한 지식은, 이미 있는 어셈블리를 디스어셈블하고, CIL 코드를 수정하고, 갱신된 코드를 재컴파일할 수 있게 해주며, 또한 CIL은 CTS(Common Type System)와 CLS(Common Language Specification. CLS ⊂ CTS ⊂ CLR)의 각 측면에 접근할 수 있도록 허용하는 유일한 .NET 프로그래밍 언어입니다. CIL 컴파일러에 의해 이해된 토큰 셋은 세 카테고리로 나누어집니다. CIL 토큰의 각 카테고리는 특정 구문을 통해 표현되며, 이러한 세 카테고리는 다음과 같습니다.

### CIL Directives
Directive들은 하나의 점 접두사에 의해 구문적으로 나타내어집니다.(.class, .assembly) 그것들은 어셈블리를 채울 이름공간, 클래스, 그리고 메서드들을 정의하기 위해 CIL 컴파일러에게 알리는 데 사용됩니다.
### CIL Attributes
종종 CIL directive들은 주어진 타입의 정의를 완전히 표현하기에 충분히 기술적이지 않지만, 그것들은 다양한 CIL attribute들로써 더욱 잘 명시될 수 있고 directive가 어떻게 처리되어야 하는지를 기술할 수 있습니다.
### CIL Opcodes
codesm 또는 opcode 연산은, 한때 .NET 어셈블리 이름공간과 타입이 CIL 코드에 대해 정의된 타입 구현 로직을 제공합니다.

CIL 코드를 사용해 어셈블리를 빌드하거나 수정할 때, peverify.exe 유틸리티를 이용해 컴파일된 바이너리 이미지가 잘 형식화된 .NET 이미지인지 검증하는 것이 권장됩니다.

</br>

## CIL 예시 #1 : Test.il

```
// mscorlib 라이브러리에 대한 외부 참조
// mscorlib.dll은 System.Console 클래스를 포함하는
// .NET 프레임워크 FCL의 핵심을 담고 있음
.assembly extern mscorlib {}
// 어셈블리의 이름
.assembly FirstApp
{
}

// 이름공간 정의
.namespace FirstApp
{
  // 이 클래스는 묵시적으로 System.Object 클래스를 상속받음
  .class private auto ansi beforefieldinit Test
  {
    // cil 키워드는 메서드가 intermediate 코드를 포함함을 나타냄
    .method public hidebysig static void Main(string[] argd) cil managed
    {
      .entrypoint	// Main 함수를 애플리케이션의 엔트리 포인트로 지정
      // 메모리 스택 크기를 1 슬롯으로 설정
      .maxstack	1
      // 문자열을 메모리로 로드함
      ldstr		"Welcome to CIL programming world“
      // 메모리로부터 한 항목을 가져온 뒤 WriteLine 메서드를 이용해 출력
      call		void [mscorlib] System.Console::WriteLine(string)
      ret
    }
  }
}
```

</br>

## CIL 예시 #2 : MathFun.il
```
.assembly extern mscorlib
{    
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )    
  .ver 2:0:0:0
}
.assembly MathFun
{    
  .ver 1:0:0:0    
  .locale “en.US“
}
.module MathFun.exe

.imagebase 0x00400000
.file alignment 0x00000200
.stackreverse 0x00100000
.subsystem 0x0003
.corflags 0x00000003

.class public auto ansi beforefieldinit MathFun extends [mscorlib]System.Object
{
  // 다음 메서드는 C# 버전과 마찬가지로 생성자 명세를 구현함
  // public Test(string name) {this.Name = name;}
  .field private string ’<Name>k__BackingField’
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
  .method public hidebysig specialname rtspecialname instance void .ctor(string name) cil managed
  {
    .maxstack	8
    IL_0000: ldarg.0
    IL_0001: call		instance void [mscorlib]System.Object::.ctor()
    IL_0006: nop
    IL_0007: nop
    IL_0008: ldarg.0
    IL_0009: ldarg.1
    IL_000a: call		instance void MathFun::set_Name(string)
    IL_000f: nop
    IL_0010: nop
    IL_0011: ret
  }

  .method public hidebysig specialname instance string get_Name() cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    .maxstack	1
    .locals init (string V_0)
    IL_0000: ldarg.0
    IL_0001: ldfld		string MathFun::’<Name>k__BackingField’       
    IL_0006: stloc.0
    IL_0007: br.s     IL_0009
    IL_0009: ldloc.0       
    IL_000a: ret
  }

  .method public hidebysig specialname instance void set_Name(string ’value’) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    .maxstack	8
    IL_0000: ldarg.0
    IL_0001: ldarg.1
    IL_0002: stfld		string MathFun::’<Name>k__BackingField’
    IL_0007: ret
  }

  .method public hidebysig instance string Display() cil managed
  {
    .maxstack	2
    .locals init ([0] string CS$1$0000)
    IL_0000: nop
    IL_0001: ldstr		“Hello “
    IL_0006: ldarg.0
    IL_0007: call		instance string MathFun::get_Name()
    IL_000c: call		string [mscorlib]System.String::Concat(string, string)
    IL_0011: stloc.0
    IL_0012: br.s		IL_0014
    IL_0014: ldloc.0
    IL_0015: ret
  }

  .method public hidebysig instance int32 Addition(int32 x, int32 y) cil managed
  {
    .maxstack	2
    .locals init ([0] int32 CS$1$0000)
    IL_0000: nop
    IL_0001: ldarg.1
    IL_0002: ldarg.2
    IL_0003: add
    IL_0004: stloc.0
    IL_0005: br.s		IL_0007
    IL_0007: ldloc.0
    IL_0008: ret
  }

  // 다음의 C# 코드와 같이 property를 정의함
  // public String Name {get; set;}
  .property instance string Name()
  {
    .get instance string MathFun::get_Name()
    .set instance void MathFun::set_Name(string)
  }
}

.class private auto ansi beforefieldinit MathFun extends [mscorlib]System.Object
{
  .method private hidebysig static void Main(string[] args) cil managed
  {
    .entrypoint
    .maxstack	4
    .locals init ([0] class MathFun obj)
    IL_0000: nop
    IL_0001: ldstr		“Ajay“
    IL_0006: newobj	instance void MathFun::.ctor(string)
    IL_000b: stloc.0
    IL_000c: ldloc.0
    IL_000d: callvirt	instance string MathFun::Display()
    IL_0012: call		void [mscorlib]System.Console::WriteLine(string)
    IL_0017: nop
    IL_0018: ldstr		“Addition is: {0}“
    IL_001d: ldloc.0
    IL_001e: ldc.i4.s	15
    IL_0020: ldc.i4.s	35
    IL_0022: callvirt	instance int32 MathFun::Addition(int32, int32)
    IL_0027: box		[mscorlib]System.Int32
    IL_002c: call		void [mscorlib]System.Console::WriteLine(string, object)
    IL_0031: nop
    IL_0032: call		valuetype [mscorlib]System.ConsoleKeyInfo [mscorlib]System.Console::ReadKey()
    IL_0037: pop
    IL_0038: ret
  }
}
```

</br>

## Assembly Directives
### .assembly extern
외부 어셈블리를 나타냅니다. 참조된 어셈블리의 public 타입들과 메서드들은 현재 어셈블리에 대해 가용하게 주어지며 구문은 다음과 같습니다.
> .assembly extern name as aliasname {}
### .assembly
어셈블리는 다음과 같이 바이너리에 친근한 이름을 명시함으로써 정의될 수 있습니다.
> .assembly CILType {}

다음과 같은, 어셈블리 블록에 대해 가용한 몇몇 sub-directive들이 존재합니다.
* .ver
* .locale
* .publickey
### .module 
다음의 *.exe와 같이, 파일들의 최종 실행 확장자를 지정합니다.
> .module MathFun.exe
### .imagebase
애플리케이션이 로드되는 베이스 주소를 설정합니다. 기본값은 0x00400000입니다.
### .stackreverse
스택 크기를 설정합니다. 기본값은 0x00100000입니다.
### .subsystem
콘솔 또는 GUI 서브시스템과 같이, 애플리케이션에 의해 사용되는 서브시스템을 나타냅니다. 콘솔 애플리케이션을 위해선 3을, GUI 애플리케이션을 위해선 2를 명시합니다.
### .corflags
IL only assembly를 규정하는 CLI 헤더 내의 런타임 플래그를 설정합니다. corflags에 대한 기본값은 1입니다.
### .maxstack
실행 중에 스택에 push될 변수들의 최대 개수를 설정합니다. 기본값은 8입니다.

</br>

## Class Directives
### .class
새로운 참조, 값, 또는 인터페이스 타입을 정의하며, 구문은 다음과 같습니다.
> attributes classname extends basetype implements interface

class directive 역시 다양한 attribute들로 꾸며지며, 다음은 가장 많이 쓰이는 목록입니다.
* abstract : 클래스가 인스턴스화될 수 없음을 나타냅니다.
* ansi와 Unicode : 문자열 포맷을 결정합니다.
* auto : 필드들의 메모리 레이아웃을 CLR이 제어합니다.
* beforefieldinit : 정적 클래스가 접근되기 전, 타입은 초기화되어야 합니다.
* private와 public : 클래스 밖에서의 가시성을 설정합니다.
### .property 
클래스에 property 멤버를 추가하며 구문은 다음과 같습니다.
> .property attributes return propertyname parameters default {body}
### .method
클래스 내의 메서드를 정의하며, 구문은 다음과 같습니다.
> .method attributes callingconv return methodname arguments {body}
#### attribute
메서드 attribute는 다음과 같은 몇몇 추가적인 attribute들을 가집니다.
* hidebysig : 이 메서드의 베이스 클래스 인터페이스를 숨깁니다.
* Specialname : get_Property와 set_Property와 같은 특수한 메서드들에 사용됩니다.
* Rtspecialname : 생성자로서 참조되는 특수한 메서드를 나타냅니다.
* Cil 또는 il : 메서드가 MSIL 코드를 담고 있습니다.
* Native : 메서드가 플랫폼 특정적 코드를 담고 있습니다.
* Managed : 구현이 관리됨을 나타냅니다.
#### .field
클래스에 대한 상태 정보인 새롭게 정의된 필드를 나타냅니다. 구문은 다음과 같습니다.
> .field attributes type fieldname

</br>

## Main() Method Directives
### .locals 
이름에 의해 가용하게 되는 지역 변수들을 선언합니다.
여기선, 다음과 같이 두 정수 타입 지역 변수들을 정의하고 있습니다.
> .locals init ([0] int32 x, [1] int32 y)

다음과 같이 클래스 생성자에 문자열 데이터를 전달함으로써 문자열 슬롯을 할당하고 있습니다.
> .locals init ([0] class MathFun obj)

</br>

## CIL 데이터 타입

![cildatatype](https://github.com/mollose/Formbook-Analysis/assets/57161613/ae407adc-725a-44b8-991f-37ac1d05f396)

</br>

## MSIL 코드 레이블
IL_XXX(예를 들어, IL_0000, IL_0002)와 같은 토큰들은 코드 레이블들이라 불리고 완전히 부가적이며, 어떤 방식으로든 이름 지어질 수 있습니다.

</br>

## MSIL Opcodes
CIL opcode들의 전체 셋은 다음 부분들로 나누어질 수 있습니다.

![msilopcodes1](https://github.com/mollose/Formbook-Analysis/assets/57161613/18a6d414-a438-4754-b1e3-625bd97d658b)

![msilopcodes2](https://github.com/mollose/Formbook-Analysis/assets/57161613/fc0f434d-1a58-4ddc-a876-694574952813)

### rem
value1을 value2로 나눈 나머지를 스택에 push합니다.
### ldelem.u1
우선 array와 index를 스택에 push한 후 사용합니다. array 내 index 위치의 unsigned int8 타입의 요소를 스택의 top에 int32 형식으로 로드합니다.
### ldelem.ref
array가 객체 참조 배열이라는 점, 그리고 스택에 요소를 로드할 때 해당 요소의 타입이 O라는 점을 제외하곤 ldelem.u1과 동일합니다.
### stelem.i1
우선 array, index, 그리고 value를 스택에 push한 후 사용합니다. array 내 주어진 index 위치의 값을 스택 내의 int8 value로 대체합니다.
### conv.u1
push된 값을 int8로 변환한 뒤, 스택에 int32 형식으로 push합니다.
### Callvirt
우선 객체 참조 obj를 스택에 push한 후 사용합니다. 이후 push된 매개변수들과 함께 메서드가 호출되고 제어는 메서드 메타데이터 토큰에 의해 참조된 obj 내의 메서드에 전달됩니다.(매개변수들이 pop될 때 obj 역시 스택에서 pop됨)
### stsfld
정적 필드의 값을 계산 스택의 값으로 변경합니다.
### newarr
배열 요소들의 수를 스택에 push한 후 사용합니다. 새 배열에 대한 참조가 스택 내로 push됩니다.
### dup
스택의 top 요소를 복사하고, 스택에 두 동일한 값들을 남겨놓습니다.
### sub.ovf 
오버플로 검사와 함께 value1으로부터 value2를 뺍니다.
### ldlen 
우선 array를 스택에 push한 후 사용합니다. array의 길이가 스택으로 push됩니다.
### 예시
”x“ 값(상수 0)을 메모리로 로드한 뒤 인덱스 ‘0’(지역 변수 x)에 저장합니다.
```
.locals init ([0] int32 x, [1] int32 i, [2] bool CS$4$0000)
IL_0000: nop
IL_0001: ldc.i4.0
IL_0002: stloc.0
```
‘i’ 값이 7보다 작을 경우 IL_0007로 분기합니다.
```
IL_001e: ldloc.1
IL_001f: ldc.i4.7
IL_0020: clt
IL_0022: stloc.2
IL_0023: ldloc.2
IL_0024: brtrue.s		IL_0007
```
### Boxing
Boxing은 값 타입을 참조 타입(System.Object)으로 명시적으로 할당하는 과정입니다. 어떤 값을 box한다면, CLR은 힙에 새 객체를 할당하고 값들을 해당 인스턴스로 복사합니다. 반대 연산은 unboxing으로, 참조 내에 주어진 값을 다시 상응하는 값 타입으로 변환합니다.
```
.locals init ([0] int32 x, [1] object bObj, [2] int32 y)
// 중략
IL_0004: ldloc.0
IL_0005: box		      [mscorlib]System.Int32
IL_000a: stloc.1
IL_000b: ldloc.1
IL_000c: unbox.any  [mscorlib]System.Int32
IL_00011: stloc.2
```

</br>

## 인터페이스
인터페이스 내에서 필드는 허용되지 않고 멤버 함수는 반드시 public, abstract, 그리고 virtual이어야 합니다.
```
.assembly CILComplexTest
{
}
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )
  .ver 4:0:0:0
}

.class interface public abstract auto ansi CILComplexTest.Repository
{
  .method public hidebysig newslot abstract virtual instance void Display() cil managed
  {
  }
}
.class public auto ansi beforefieldinit CILComplexTest.test extends [mscorlib]System.Object implements CILComplexTest.Repository
{
  .method public hidebysig newslot virtual final instance void Display() cil managed
  {
    .maxstack	8
    IL_0000: nop
    IL_0001: ldstr	“Hello“
    IL_0006: call		void [mscorlib]System.Console::WriteLine(string)
    IL_000b: nop
    IL_000c: ret
  }
}

.class private auto ansi beforefieldinit CILComplexTest.Program extends [mscorlib]System.Object
{
  .method private hidebysig static void Main(string[] args) cil managed
  {
    .entrypoint
    .maxstack	8
    IL_0000: nop
    IL_0001: newobj	instance void CILComplexTest.test::.ctor()
    IL_0006: call		instance void CILComplexTest.test::Display()
    IL_000b: nop
    IL_000c: ret
  }

  .method public hidebysig specialname rtsspecialname instance void .ctor() cil managed
  {
    .maxstack	8
    IL_0000: ldarg.0
    IL_0001: call		instance void [mscorlib]System.Object::.ctor()
    IL_0006: ret
  }
}
```

</br>

## MSIL 코드 생성
Visual Studio Command Prompt를 열고 다음 명령을 실행하여 이미 있는 C# 코드 명령어를 MSIL 코드로 변환할 수 있습니다.
> ILDASM CILComplexTest.exe /out:test.il

</br>

## 메타데이터 및 자동 기술 구성 요소
코드를 실행하면 런타임은 메타데이터를 메모리로 로드한 다음 참조하여 해당 코드의 클래스, 멤버, 상속 등에 대한 정보를 검색합니다. 메타데이터는 언어와 무관하게 코드에 정의된 모든 형식과 멤버를 기술하며 다음과 같은 정보를 저장합니다.
### 어셈블리 기술 내용
* ID(이름, 버전, 문화권, 공개 키)
* 내보낸 형식
* 이 어셈블리가 종속된 다른 어셈블리
* 실행하는 데 필요한 보안 권한
### 형식 기술 내용
* 이름, 표시 여부, 기본 클래스 및 구현된 인터페이스
* 멤버(메서드, 필드, 속성, 이벤트, 중첩 형식)
### 특성
* 형식과 멤버를 수정하는 추가 설명적 요소
### 메타데이터 및 PE 파일 구조
메타데이터는 .NET PE 파일의 한 섹션에 저장되고 MSIL은 PE 파일의 다른 섹션에 저장됩니다. PE 파일의 메타데이터 부분에는 테이블과 힙 데이터 구조가 있으며 MSIL 부분에는 MSIL 및 PE 파일의 메타데이터 부분을 참조하는 메타데이터 토큰이 있습니다.
#### 메타데이터 테이블 및 힙
각 메타데이터 테이블은 프로그램 요소에 대한 정보를 보유합니다. 예를 들어, 한 메타데이터 테이블은 코드의 클래스를 나타내고 다른 테이블은 필드를 나타내는 식이며, 메타데이터 테이블은 다른 테이블과 힙을 참조합니다. 예를 들어, 클래스의 메타데이터 테이블은 메서드의 테이블을 참조합니다. 또한 메타데이터는 문자열, blob, 사용자 문자열 및 GUID라고 하는 네 가지 힙 구조에 정보를 저장합니다. 형식과 멤버의 이름을 지정하는데 사용되는 모든 문자열은 문자열 힙에 저장됩니다. 예를 들어 메서드 테이블은 특정 메서드의 이름을 직접 저장하지는 않지만 문자열 힙에 저장된 메서드의 이름을 가리킵니다.
#### 메타데이터 토큰 
메타데이터 테이블의 각 행은 메타데이터 토큰에 의해 PE 파일의 MSIL 부분에서 고유하게 식별됩니다. 메타데이터 토큰은 MSIL에 유지되고 특정 메타데이터 테이블을 참조하는 포인터와 개념적으로 비슷합니다. 메타데이터 토큰은 4바이트 숫자이며, 최상위 바이트는 특정 토큰이 참조하는 메타데이터 테이블(메서드, 형식 등)을 나타내고 나머지 3바이트는 메타데이터 테이블의 행 중에서 현재 나타내고 있는 프로그래밍 요소에 해당하는 행을 지정합니다.

</br>

## Opcodes.Ldtoken 필드
메타데이터 토큰을 런타임 표현으로 변환하여 계산 스택으로 푸시합니다.

</br>

## Type.GetTypeFromHandle(RuntimeTypeHandle)
지정된 타입 핸들이 참조하는 타입을 가져옵니다.
> public static Type GetTypeFromHandle(RuntimeTypeHandle handle);
* handle : 타입을 참조하는 객체입니다.
* 반환 : RuntimeTypeHandle에서 참조하는 타입이거나 null의 Value 속성이 handle인 경우 null입니다.

</br>

## ResourceManager(Type)
지정된 타입 객체의 정보를 기초로 위성 어셈블리에서 리소스를 찾는 ResourceManager 클래스의 새 인스턴스를 초기화합니다.
> public ResourceManager (Type resourceSource)
* resourceSource : 리소스 관리자가 .resources 파일을 찾는데 필요한 모든 정보를 파생시키는 타입입니다.

</br>

## .resources 파일의 리소스
System.Resources.ResourceWriter 클래스를 사용하여 프로그래밍 방식으로 코드에서 직접 이진 리소스(.resources) 파일을 만들 수 있습니다. 리소스 파일 생성기(resgen.exe)를 사용하여 텍스트 파일 또는 .resx 파일에서 .resources 파일을 만들 수도 있습니다. .resources 파일에는 문자열 데이터 외에 이진 데이터(바이트 배열) 및 객체 데이터가 포함될 수 있습니다.

</br>

## .resources 파일에서 리소스 검색
위성 어셈블리에서 리소스를 배포하지 않더라도 ResourceManager 객체를 사용하여 .resources 파일에서 직접 리소스에 액세스할 수 있습니다.
### 1) .resources 파일 배포
애플리케이션 어셈블리 및 위성 어셈블리에 .resources 파일을 포함하는 경우, 각 위성 어셈블리는 같은 파일 이름을 갖지만 위성 어셈블리의 문화권을 반영하는 하위 디렉터리에 배치됩니다. 반면, .resources 파일에서 직접 리소스에 액세스할 경우에는 일반적으로 애플리케이션 디렉터리의 하위 디렉터리인 단일 디렉터리에 모든 .resources 파일을 배치할 수 있습니다. 앱의 기본 .resources 파일의 이름은 문화권에 대한 암시 없이 루트 이름으로만 구성됩니다. 지역화된 각 문화권에 대한 리소스는 이름이 루트 이름과 문화권으로 구성된 파일에 저장됩니다.
### 2) 리소스 관리자 사용
리소스를 만들어서 적절한 디렉터리에 배치했으면, ResourcesManager 메서드를 호출하여 리소스를 사용할 수 있도록 CreateFileBasedResourceManager(String, String, Type) 객체를 만돕니다. 첫 번째 매개 변수는 앱의 기본 .resources 파일의 루트 이름을 지정하고 두 번째 매개 변수는 리소스의 위치를 지정합니다. 세 번째 매개 변수는 사용할 ResourceSet 구현을 지정합니다. 세 번째 매개 변수가 null인 경우 기본 런타임 ResourceSet이 사용됩니다.

</br>

## 애플리케이션 도메인 및 어셈블리
어셈블리에 포함되어 있는 코드를 실행하려면 먼저 해당 어셈블리를 애플리케이션 도메인에 로드해야 합니다. 일반적으로 애플리케이션을 실행하면 여러 어셈블리가 애플리케이션 도메인에 로드됩니다.
* 어셈블리가 도메인 중립적으로 로드된 경우 동일한 보안 권한 보유 집합을 공유하는 모든 애플리케이션 도메인에서 동일한 JIT 컴파일된 코드를 공유할 수 있습니다. 그러나 어셈블리를 프로세스에서 로드할 수는 없습니다.
* 어셈블리가 도메인 중립적으로 로드되지 않은 경우 해당 어셈블리가 로드된 모든 애플리케이션 도메인에서 어셈블리를 JIT 컴파일해야 합니다. 그러나 어셈블리가 로드된 모든 애플리케이션 도메인을 언로드하여 어셈블리를 언로드할 수는 있습니다.

</br>

## 애플리케이션 도메인과 스레드
애플리케이션 도메인과 스레드 간에는 일대일 상관관계가 없습니다. 일부 스레드는 어느 시점에서든 단일 애플리케이션 도메인에서 실행될 수 있으며, 특정 스레드는 단일 애플리케이션 도메인으로 제한되지 않습니다. 즉, 스레드는 크로스 애플리케이션 도메인 경계에 종속되지 않고 각 애플리케이션 도메인에 대해 새 스레드가 만들어지지 않습니다.

</br>

## AppDomain.CurrentDomain 속성
현재 스레드에 대한 현재 애플리케이션 도메인을 가져옵니다.

</br>

## AppDomain.Load(Byte[]) 
내보낸 어셈블리가 들어 있는 COFF(Common Object File Format) 기반 이미지를 사용한 어셈블리를 로드합니다.

</br>

## Assembly.GetTypes 메서드
> public virtual Type[] GetTypes();
* 반환 : 이 어셈블리에 정의되어 있는 모든 타입이 포함된 배열입니다.

</br>

## Type.GetMethods 메서드
> public System.Reflection.MethodInfo[] GetMethods();
* 반환 : 현재 타입에 대해 정의된 모든 public 메서드를 나타내는 MethodInfo 객체들의 배열입니다.

</br>

## 파일 기반 리소스 로딩 
파일 Project.Properties.Resources.resources의 경우,
* 프로젝트명 : Project
* 이름공간명 : Project
* 클래스명 : Properties
* 리소스 위치 : Project\bin\Debug\Resources\Project.Properties.Resources.resources
* 로딩 방식
  > ResourceManager rsm = ResourceManager.CreateFileBasedResourceManager(“Project.Properties.Resources“, “Resources“, null);
