# 코딩 컨벤션

[메인 지시서](../CLAUDE.md)의 보조 문서. `src/EnglishTraining`(WPF, .NET 8, C#) 코드 작성 시 따르는 규칙을 정리한다. 새 규칙이 필요해지면 이 문서를 먼저 갱신하고 코드에 반영한다.

---

## 1. 네이밍

| 대상 | 규칙 | 예 |
|---|---|---|
| 클래스/레코드/열거형 | PascalCase | `TextSegmenter`, `LearningExpression` |
| 인터페이스 | `I` + PascalCase | `IExpressionRepository` |
| 공개 메서드/프로퍼티 | PascalCase | `TryBuildSections`, `FontSize` |
| private 필드 | `_camelCase` | `_repository`, `_viewModel`, `_activeSpan` |
| 지역 변수/매개변수 | camelCase | `sourceText`, `normalizedText` |

## 2. 파일/폴더 구조

* 클래스 하나당 파일 하나, 파일명은 클래스명과 동일 (`TopicViewModel.cs` → `TopicViewModel`).
* 네임스페이스는 폴더와 1:1 대응한다: `Models`, `Services`, `ViewModels`, `Views`, `Controls`, `Converters`.
* WPF 창은 XAML(`FooWindow.xaml`)과 코드비하인드(`FooWindow.xaml.cs`)를 같은 이름으로 짝지어 `Views/`에 둔다. ViewModel이 필요한 경우 `ViewModels/FooViewModel.cs`로 대응시킨다(반드시 1:1은 아니다 — `MainViewModel`은 `ReadingWindow` 하나만 위해 존재하지만, `TopicViewModel`은 여러 항목에 재사용된다).

## 3. MVVM / 계층 책임

* **Models**: 순수 데이터. UI/영속성 코드를 포함하지 않는다(`LearningExpression`, `Topic`, `InterpretationEntry` 등).
* **Services**: 파일 I/O, JSON 직렬화, 텍스트 파싱/정규화, 세그멘테이션 등 UI와 무관한 로직(`LessonFolderLoader`, `TextSegmenter`, `TextNormalizer`, `*MarkdownParser`).
* **ViewModels**: `ViewModelBase`(`INotifyPropertyChanged`)를 상속하고, 변경 알림은 `SetField(ref field, value)`(필드 기반) 또는 `OnPropertyChanged(name)`(계산 프로퍼티)로 처리한다. 커맨드는 `RelayCommand`(`ICommand` 구현)를 사용한다.
* **Views**: XAML 위주. 코드비하인드에는 다음만 둔다 — `Popup`/`MessageBox`/`OpenFileDialog`/`OpenFolderDialog` 등 순수 WPF API 호출, 이벤트 핸들러에서 ViewModel 메서드 호출, `InitializeComponent` 이후의 창 조립(`BuildDocument()` 등). 데이터 가공·저장 로직은 ViewModel/Service로 내린다.
* **Controls**: 재사용 가능한 커스텀 WPF 컨트롤(`ExpressionSpan`).
* **Converters**: `IValueConverter` 구현(`NullToVisibilityConverter`).

## 4. 제네릭/추상화 사용 기준

똑같은 구조를 가진 케이스가 **3개 이상 동시에 필요할 때만** 제네릭으로 묶는다. 그 외에는 중복을 감수하더라도 구체 클래스로 둔다 — 아직 쓰이지 않는 유연성을 미리 만들지 않는다. (한때 해석/영작/표현 관리 창 3개가 `IEntry`/`JsonEntryRepository<T>`/`EntryManagementViewModel<T>` 제네릭으로 묶여 있었으나, §31 읽기 전용 전환으로 관리 창 자체가 모두 제거되면서 이 제네릭 계층도 함께 없앴다 — 지금은 `InterpretationEntry`/`WritingEntry`가 각각 독립된 구체 클래스다.)

## 5. 데이터 저장

학습 데이터(주제·단어·영작)는 앱이 저장하지 않는다 — 설정된 데이터 폴더(§31.4)의 레슨 파일(§30)을 읽기만 한다. 앱이 실제로 쓰는 파일은 `%LOCALAPPDATA%\EnglishTraining\settings.json`과 저장소 안 `data/today.md`뿐이다.

* JSON 직렬화(`settings.json`)는 `System.Text.Json`, `WriteIndented = true`.
* 저장소 안 `data/`, `doc/sample-*.md` 등은 예시/원본 텍스트이지 앱이 관리하는 상태 파일이 아니다. 단, "단어/문장 등록"처럼 저장소 안 `data/`에 결과를 남기는 것 자체가 목적인 기능은 예외로 `RepoPaths.FindDataDirectory()`로 리포 루트를 찾아 `data/`에 직접 쓴다(예: `TodayEnglishFile` → `data/today.md`, §27 참고) — 이 경우도 실행 파일 상대 경로가 아니라 `EnglishTraining.sln` 위치 기준으로 찾는다.
* 데이터 폴더 경로(`AppSettings.DataFolder`)와 마지막 선택 주제(`LastSelectedTopicFile`/`LastSelectedTopicTitle`)는 `AppSettingsStore`를 통해서만 읽고 쓴다 — write-through(설정 즉시 저장) 패턴을 그대로 따른다.
* API 키 등 시크릿이 필요한 기능을 추가할 때는 코드에 하드코딩하거나 저장소(git)에 커밋하지 않는다. 사용자가 앱 내 설정 창을 통해 입력하고 `AppSettingsStore`에만 저장하는 방식을 따른다 (계획 중인 예: 실시간 번역 기능, [doc/common-management.md](common-management.md) §26.5 참고 — 현재는 보류 상태로 코드에는 없음).

## 6. 텍스트/파일 파싱

* 레슨 파일(§30)은 `## 기사 제목` + `### Text`/`### Words`/`### Writing` 구조를 기본 틀로 삼는다(`LessonMarkdownParser`). `### Text`의 링크 정리·빈 줄 정리는 `MarkdownSectionSplitter.CleanBody`를, `### Words`의 단어 줄 인식은 `TodayEnglishParser.TryParseWordLine`을 재사용한다 — 같은 로직을 새로 만들지 않는다.
* 표현 매칭용 정규화는 `TextNormalizer`(소문자화 + 구두점 제거) 하나로 통일한다 — 매칭/폴더 읽기 등 다른 곳에서 별도 정규화 로직을 만들지 않는다.

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
