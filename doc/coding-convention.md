# 코딩 컨벤션

[메인 지시서](../CLAUDE.md)의 보조 문서. `src/EnglishTraining`(WPF, .NET 8, C#) 코드 작성 시 따르는 규칙을 정리한다. 새 규칙이 필요해지면 이 문서를 먼저 갱신하고 코드에 반영한다.

---

## 1. 네이밍

| 대상 | 규칙 | 예 |
|---|---|---|
| 클래스/레코드/열거형 | PascalCase | `TextSegmenter`, `LearningExpression` |
| 인터페이스 | `I` + PascalCase | `IEntry`, `ITopicRepository`, `IExpressionRepository` |
| 공개 메서드/프로퍼티 | PascalCase | `TryBuildSections`, `FontSize` |
| private 필드 | `_camelCase` | `_repository`, `_viewModel`, `_activeSpan` |
| 지역 변수/매개변수 | camelCase | `sourceText`, `normalizedText` |
| 제네릭 타입 매개변수 | `T` (엔티티 하나만 다룰 때) | `JsonEntryRepository<T>`, `EntryManagementViewModel<T>` |

## 2. 파일/폴더 구조

* 클래스 하나당 파일 하나, 파일명은 클래스명과 동일 (`TopicViewModel.cs` → `TopicViewModel`).
* 네임스페이스는 폴더와 1:1 대응한다: `Models`, `Services`, `ViewModels`, `Views`, `Controls`, `Converters`.
* WPF 창은 XAML(`FooWindow.xaml`)과 코드비하인드(`FooWindow.xaml.cs`)를 같은 이름으로 짝지어 `Views/`에 둔다. 관리 창처럼 ViewModel이 필요한 경우 `ViewModels/FooViewModel.cs`로 대응시킨다(반드시 1:1은 아니며, 여러 창이 제네릭 ViewModel 하나를 공유할 수 있다 — 아래 §4).

## 3. MVVM / 계층 책임

* **Models**: 순수 데이터. UI/영속성 코드를 포함하지 않는다(`LearningExpression`, `Topic`, `InterpretationEntry` 등).
* **Services**: 파일 I/O, JSON 직렬화, 텍스트 파싱/정규화, 세그멘테이션 등 UI와 무관한 로직(`JsonEntryRepository<T>`, `TextSegmenter`, `TextNormalizer`, `*MarkdownParser`).
* **ViewModels**: `ViewModelBase`(`INotifyPropertyChanged`)를 상속하고, 변경 알림은 `SetField(ref field, value)`(필드 기반) 또는 `OnPropertyChanged(name)`(계산 프로퍼티)로 처리한다. 커맨드는 `RelayCommand`(`ICommand` 구현)를 사용한다.
* **Views**: XAML 위주. 코드비하인드에는 다음만 둔다 — `Popup`/`MessageBox`/`OpenFileDialog` 등 순수 WPF API 호출, 이벤트 핸들러에서 ViewModel 메서드 호출, `InitializeComponent` 이후의 창 조립(`BuildDocument()` 등). 데이터 가공·저장 로직은 ViewModel/Service로 내린다.
* **Controls**: 재사용 가능한 커스텀 WPF 컨트롤(`ExpressionSpan`).
* **Converters**: `IValueConverter` 구현(`NullToVisibilityConverter`).

## 4. 제네릭/추상화 사용 기준

똑같은 구조(예: `Id` + `Text` + 카테고리별 필드를 가진 JSON 목록을 CRUD하는 창)가 **3개 이상 동시에 필요할 때만** 제네릭으로 묶는다(`IEntry`, `JsonEntryRepository<T>`, `EntryManagementViewModel<T>` — 해석/영작/표현 관리 창 3개가 동시에 생기면서 도입). 그 외에는 중복을 감수하더라도 구체 클래스로 둔다 — 아직 쓰이지 않는 유연성을 미리 만들지 않는다. `문장 관리`(`Topic`/`ITopicRepository`/`JsonTopicRepository`)는 하나뿐이라 지금도 제네릭으로 통합하지 않는다.

## 5. 데이터 저장

* JSON 직렬화는 `System.Text.Json`, `WriteIndented = true`.
* 사용자가 앱 내에서 추가/수정/삭제하는 데이터(주제, 해석/영작/표현)는 `%LOCALAPPDATA%\EnglishTraining\*.json`에 저장한다 — 실행 파일 상대 경로(`./data`)를 쓰지 않는다(설치 위치·실행 디렉터리에 영향받지 않도록).
* 저장소 안 `data/`, `doc/sample-*.md` 등은 가져오기(import)용 예시/원본 텍스트일 뿐 앱이 쓰고 지우는 상태 파일이 아니다.
* 파일 저장소 클래스(`Json*Repository`)는 `FilePath`, `Save()`, `SaveAs(path)`, `Open(path)`를 공통으로 제공한다.

## 6. 텍스트/파일 파싱

* 가져오기용 md 형식은 "`#`/`##`/`###` 제목 한 줄 + `라벨: 값` 줄들"을 기본 틀로 삼는다(`TopicMarkdownParser`, `LabeledTextParser`, `*MarkdownParser`). 새 카테고리를 추가할 때도 이 틀을 우선 재사용한다.
* 표현 매칭용 정규화는 `TextNormalizer`(소문자화 + 구두점 제거) 하나로 통일한다 — 매칭/가져오기 등 다른 곳에서 별도 정규화 로직을 만들지 않는다.

## 7. 테스트

* xUnit, 테스트 대상 클래스당 `tests/EnglishTraining.Tests/{ClassName}Tests.cs` 하나.
* 입력 조합이 여러 개인 순수 함수(`TextNormalizer.Normalize` 등)는 `[Theory]`+`[InlineData]`, 그 외는 `[Fact]`.
* UI(마우스오버, 팝업 위치, 창 전환)는 유닛 테스트로 검증하지 않고 `dotnet run`으로 직접 확인한다. 대신 그 UI가 의존하는 로직(세그멘테이션, 매칭 우선순위, 팝업 섹션 조립, 저장소 CRUD)은 반드시 유닛 테스트로 커버한다.

## 8. 커밋 메시지

* `feat:`, `fix:`, `docs:`, `refactor:`, `test:` 등 [Conventional Commits](https://www.conventionalcommits.org/) 스타일 접두사 + 한글 또는 영어 설명. 예: `feat: 해석/영작/표현 가져오기·내보내기 추가`, `docs: 문장 관리 md 형식(다중 주제) 반영`.
* 지시서(`CLAUDE.md`, `doc/*.md`)만 바뀐 경우는 `docs:` 커밋으로 코드 변경과 분리한다.
* 사용자가 명시적으로 요청했을 때만 커밋한다(자동으로 커밋하지 않음).

## 9. 포맷팅

* 4칸 들여쓰기, Allman 스타일 중괄호(여는 중괄호를 다음 줄에) — Visual Studio/`dotnet format` 기본값을 따른다.
* 주석은 "왜"가 코드만으로 드러나지 않을 때만 한 줄로 남긴다. 무엇을 하는지 설명하는 주석, 여러 줄짜리 문서화 주석은 쓰지 않는다.
