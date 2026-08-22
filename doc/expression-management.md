# 해석/영작/표현 관리 — 학습 데이터 관리 창

[메인 지시서](../CLAUDE.md)의 보조 문서. 읽기 창의 학습 팝업에 쓰이는 해석/영작/표현 데이터를 각각 관리하는 세 창("해석 관리", "영작 관리", "표현 관리")의 구조를 다룬다. 화면에 표시할 예문(주제) 자체는 [문장 관리](sentence-management.md)를 참고하고, 팝업에 데이터가 어떻게 조합되어 보이는지는 [공통 관리](common-management.md) §5~7, §12를 참고한다.

---

## 1. 개요

해석/영작/표현 데이터는 예전에는 `expressions.json` 한 파일에 합쳐져 있었지만, 지금은 각각 독립된 파일로 분리되어 있고 전용 관리 창을 통해 추가·수정·삭제할 수 있다.

| 관리 창 | 파일 | 필드 |
|---|---|---|
| 해석 관리 | `%LOCALAPPDATA%\EnglishTraining\interpretations.json` | 표현(`Text`), 해석(`Ko`) |
| 영작 관리 | `%LOCALAPPDATA%\EnglishTraining\writings.json` | 표현(`Text`), 설명(`Description`), 예문(`Example`) |
| 표현 관리 | `%LOCALAPPDATA%\EnglishTraining\expressions.json` | 표현(`Text`), 의미(`Meaning`), 사용법(`Usage`), 예문(`Example`) |

세 창 모두 읽기 창(메인 윈도우)의 **관리** 메뉴에서 연다.

```text
읽기 창
   ↓
"관리" 메뉴 클릭
   ↓
문장 관리 / 해석 관리 / 영작 관리 / 표현 관리 중 선택
   ↓
해당 창에서 추가·수정·삭제
   ↓
창을 닫으면 읽기 창이 최신 데이터로 갱신됨(마우스오버 팝업에 즉시 반영)
```

---

## 2. 화면 구성 (세 창 공통)

```text
┌──────────────────────────────────────────────┐
│ 파일   해석(또는 영작/표현)                    │
├──────────────────────────────────────────────┤
│ [신규] [가져오기] [삭제]                       │
├───────────────┬──────────────────────────────┤
│ look           │ 표현                          │
│ look forward   │ look forward to               │
│ look forward to│                                │
│ wondering if   │ 해석                          │
│                │ ~을 기대하다, ~을 고대하다    │
├───────────────┴──────────────────────────────┤
│ C:\Users\...\interpretations.json (759 B)     │
└──────────────────────────────────────────────┘
```

* 최상단 메뉴: `파일`(열기/저장/다른 이름으로 저장), `<해석|영작|표현>`(신규/가져오기/내보내기/삭제) — [문장 관리](sentence-management.md) §4와 동일한 구조.
* 좌측 목록: 등록된 표현(`Text`) 목록. 다중 선택 가능(삭제용).
* 우측: 선택된 항목의 필드를 편집하는 영역. 창마다 필드 구성이 다르다(위 §1 표 참고).
* 하단: 현재 저장 파일의 `경로 (크기)` 상태 표시줄.

---

## 3. 신규 / 삭제

* **신규**: 표현(`Text`)을 입력받아 나머지 필드가 빈 항목을 추가하고 선택 상태로 만든다. 이후 우측에서 필드를 채운다.
* **삭제**: 좌측 목록에서 선택(다중 선택 가능)한 항목을 확인 후 삭제한다.

---

## 4. 파일 열기 / 저장 / 다른 이름으로 저장

문장 관리 §4.1과 동일하게 동작한다 — `열기`는 다른 JSON 파일로 전체 목록을 교체하고, `저장`은 현재 파일에, `다른 이름으로 저장`은 새 파일에 기록하며 이후 저장 대상도 그 파일로 바뀐다.

---

## 5. 가져오기 / 내보내기

문장 관리의 `.md` 가져오기/내보내기([sentence-management.md](sentence-management.md) §6)와 같은 방식으로, 항목 하나를 텍스트 파일로 주고받을 수 있다.

* **가져오기**: `.md` 파일을 선택하면 그 내용을 새 항목으로 추가한다.
* **내보내기**: 좌측에서 선택된 항목 하나를 `.md` 파일로 저장한다. 선택된 항목이 없으면 안내 메시지를 표시한다.

### md 파일 형식

첫 줄의 `# 표현`이 `Text`가 되고, 그 아래 `라벨: 값` 형태의 줄들이 각 필드가 된다. 라벨 뒤의 내용은 다음 라벨이 나오기 전까지 여러 줄을 포함할 수 있다. 창마다 사용하는 라벨은 다음과 같다.

| 관리 창 | 라벨 |
|---|---|
| 해석 관리 | `해석:` |
| 영작 관리 | `설명:`, `예문:` |
| 표현 관리 | `의미:`, `사용법:`, `예문:` |

샘플 파일: [doc/sample-interpretation.md](sample-interpretation.md), [doc/sample-writing.md](sample-writing.md), [doc/sample-expression.md](sample-expression.md)

```markdown
# would like to

해석: ~하고 싶다
```

```markdown
# would like to

설명: 정중하게 원하는 것을 표현할 때 사용
예문: I would like to know more about this.
```

```markdown
# would like to

의미: ~하고 싶다 (want to의 정중한 표현)
사용법: would like to + 동사원형
예문: I would like to know more about this.
```

세 파일 모두 같은 표현(`would like to`)을 예시로 쓴다 — 세 관리 창에 각각 가져오면 해석/영작/표현 데이터가 `would like to` 기준으로 자동 병합되어(아래 §6) 읽기 창에서 하나의 학습 팝업으로 합쳐진다.

---

## 6. 읽기 창과의 데이터 병합

읽기 창은 시작할 때, 그리고 세 관리 창 중 하나가 닫힐 때마다 세 파일의 내용을 **정규화된 `Text` 기준으로 병합**해 학습 표현 목록(`LearningExpression`, [공통 관리](common-management.md) §12)을 다시 만든다.

* 같은 표현이 세 파일 중 한 곳에만 있어도 등록된 표현으로 인식되며, 팝업에는 실제로 데이터가 있는 항목만 표시된다([공통 관리](common-management.md) §20 예외 처리).
* 예를 들어 `look`이 `interpretations.json`에만 있으면, `look`에 마우스를 올렸을 때 해석 정보만 뜨고 영작/표현 섹션은 표시되지 않는다.
* 표현 매칭의 정규화·최장 일치 규칙([공통 관리](common-management.md) §26.4)은 병합 후에도 그대로 적용된다 — 세 파일 중 어디에 있든 `Text`가 같으면(정규화 기준) 같은 표현으로 취급된다.

---

## 7. 기본 데이터

앱을 처음 실행하면(파일이 없으면) 아래 예시 데이터로 세 파일이 자동 생성된다. 코드상 위치는 `src/EnglishTraining/Services/DefaultLearningData.cs`.

* 해석: `look`, `look forward`, `look forward to`, `wondering if`, `as far as I know`
* 영작: `look forward to`, `wondering if`
* 표현: `look forward to`, `be supposed to`, `as far as I know`
