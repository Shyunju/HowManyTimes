# UGESystem 빠른 시작 가이드

이 가이드는 UGESystem의 핵심 기능을 사용하여, 플레이어의 선택에 따라 분기되는 간단한 대화 이벤트를 처음부터 만드는 과정을 안내합니다.

---

## 1단계: 씬(Scene) 기본 설정

가장 먼저, UGESystem이 동작할 수 있는 환경을 만듭니다.

#### 1-1. 시스템 매니저 생성

1.  새로운 씬(`UGESystemExample`)을 엽니다.
2.  상단 메뉴 바에서 **Tools > UGESystem > Create System Object**를 클릭합니다.
3.  Hierarchy 창에 `UGESystem` 게임 오브젝트가 생성된 것을 확인합니다.
    *   **설명:** 이 오브젝트는 이벤트 실행, UI, 캐릭터, 사운드 등을 관리하는 모든 핵심 매니저들을 포함하고 있습니다.

#### 1-2. 필수 UI 설정

1.  `Assets/UGESystem/Core/Prefabs/UI/` 폴더에 있는 `VNEventCanvas` 프리팹을 Hierarchy 창으로 드래그하여 씬에 추가합니다.
2.  Hierarchy에서 `UGESystem` 오브젝트를 선택하고, Inspector 창에서 **UGE UI Manager** 컴포넌트를 찾습니다.
3.  `VNEventCanvas`의 하위 오브젝트들을 **UGE UI Manager**의 각 필드에 맞게 드래그하여 연결합니다.
    *   **Dialogue Canvas:** `VNEventCanvas/Panel_Dialogue`
    *   **Dialogue Text:** `VNEventCanvas/Panel_Dialogue/Txt_Dialogue`
    *   **Character Name Text:** `VNEventCanvas/Panel_Dialogue/Txt_CharacterName`
    *   **Choice Button Prefab:** `VNEventCanvas/Panel_Choices/BTN_Choice`
    *   **Choice Button Parent:** `VNEventCanvas/Panel_Choices/ChoiceGroup`

---

## 2단계: 캐릭터 데이터베이스 설정

이벤트에 등장할 캐릭터 정보를 중앙에서 관리하는 데이터베이스를 만듭니다.

1.  `Project` 창에서 `Assets/Resources/` 폴더가 없다면 생성합니다. 그 안에 `UGESystem/CharacterData` 폴더를 차례로 생성합니다.
    *   **중요:** `Resources` 폴더는 Unity의 특수 폴더로, 이 경로를 지켜야 시스템이 에셋을 동적으로 불러올 수 있습니다.
2.  `CharacterData` 폴더에서 우클릭 후, **Create > UGESystem > Character Database**를 선택하여 `CharacterDatabase` 에셋을 생성합니다.
3.  생성된 에셋을 선택하고 Inspector 창에서 `Characters` 리스트에 두 명의 캐릭터를 추가합니다.
    *   **요소 0:**
        *   **Character ID:** `guard`
        *   **Name:** `경비병`
        *   **Expressions:** 리스트를 열고, `+` 버튼을 두 번 눌러 두 개의 표정을 추가합니다.
            *   `Expression Name`: `Default`
            *   `Expression Name`: `Angry`
    *   **요소 1:**
        *   **Character ID:** `player`
        *   **Name:** `플레이어`

---

## 3단계: 대화 이벤트(GameEvent) 제작

실제 대사의 내용과 분기를 구성하는 `GameEvent` 에셋을 만듭니다.

1.  `Assets/Resources/UGESystem/` 폴더 안에 `EventSO` 폴더를 생성합니다.
2.  `EventSO` 폴더에서 우클릭 후, **Create > UGESystem > Game Event**를 선택하여 `GE_Guard_First_Talk` 이름으로 에셋을 생성합니다.
3.  `GE_Guard_First_Talk` 에셋을 선택하고 Inspector 창에서 `Add Command` 버튼을 눌러 아래 순서대로 커맨드를 추가하고 내용을 채웁니다.
    1.  **Dialogue:**
        *   `Character Name`: `guard`
        *   `Dialogue Text`: `What brings you here?`
    2.  **Choice:**
        *   `Choices` 리스트의 `+` 버튼을 눌러 선택지 2개를 추가합니다.
        *   **요소 0:** `Text` - `I'm looking around the village.`, `Target Label` - `Friendly`
        *   **요소 1:** `Text` - `Where is the weapon shop?`, `Target Label` - `Suspicious`
    3.  **Label:**
        *   `Label Name`: `Friendly`
    4.  **Dialogue:**
        *   `Character Name`: `guard`
        *   `Dialogue Text`: `Alright. Feel free to look around.`
    5.  **End**
    6.  **Label:**
        *   `Label Name`: `Suspicious`
    7.  **Dialogue:**
        *   `Character Name`: `guard`
        *   `Expression`: `Angry`
        *   `Dialogue Text`: `State your business, quickly!`
    8.  **End**

---

## 4단계: 스토리보드 구성 및 실행

`GameEvent`들을 연결하고 실행 규칙을 정의하는 스토리보드를 만듭니다.

1.  `Assets/Resources/UGESystem/` 폴더 안에 `Storyboards` 폴더를 생성합니다.
2.  `Storyboards` 폴더에서 우클릭 후, **Create > UGESystem > Storyboard**를 선택하여 `MainQuest` 이름으로 에셋을 생성합니다.
3.  상단 메뉴 **Tools > UGESystem > Storyboard Editor**를 클릭하여 스토리보드 에디터 창을 엽니다.
4.  `MainQuest` 에셋을 Project 창에서 에디터 창으로 드래그 앤 드롭합니다.
5.  에디터 빈 공간에서 우클릭 후 `Create Node`를 선택하여 새 노드를 만듭니다.
6.  생성된 노드를 선택하고, Inspector 창의 `Game Event` 필드에 `GE_Guard_First_Talk` 에셋을 연결합니다.
7.  씬에 `3D Object > Cube`를 하나 생성하고, `Box Collider` 컴포넌트의 `Is Trigger`를 체크합니다.
8.  `Cube` 오브젝트에 **UGE Event Task Runner** 컴포넌트를 추가합니다.
9.  `Task Runner` 컴포넌트의 `Storyboard` 필드에 `MainQuest` 에셋을 연결합니다.
10. `Play` 버튼을 누르고, 플레이어(또는 카메라)를 `Cube` 트리거 안으로 이동시켜 이벤트가 시작되는지, 선택지에 따라 분기가 잘 동작하는지 확인합니다.