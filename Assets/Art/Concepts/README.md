# FarmGame2D 콘셉트 에셋

이 폴더의 이미지는 2026년 11월 7일에 만든 디자인 방향 확인용 시안이다. 실제 게임 에셋으로 사용하기 전에 픽셀 정리, 정확한 셀 재배치, 배경 투명 처리와 Unity Sprite Editor 슬라이싱이 필요하다.

## 파일 목록

- `Characters/player_pair_spritesheet_concept_v1.png`: 남성형·여성형 플레이어, 탑다운 4방향 대기·걷기 시안
- `NPCs/village_npcs_spritesheet_concept_v1.png`: 안내자, 씨앗 상점 주인, 노년 주민, 젊은 주민 시안
- `UI/dialogue_info_ui_concept_v1.png`: 대화창, 안내창, 퀘스트 알림과 공통 장식 시안
- `World/Village/village_layout_a_plaza_loop_v1.png`: 광장 중심 순환형 마을 시안
- `World/Village/village_layout_b_riverside_v1.png`: 강변 자연형 마을 시안
- `World/Village/village_layout_c_main_street_v1.png`: 중심가 일자형 마을 시안

## 생성 정보

- 도구 모드: OpenAI ImageGen, 새 이미지 생성
- 공통 방향: 따뜻한 농촌 분위기, 짙은 갈색 외곽선, 흙색·크림·저채도 녹색 중심의 픽셀아트
- 플레이어 프롬프트 요약: 능력 차이가 없는 남성형·여성형 농부를 같은 크기와 프레임 수로 만들고, 아래·왼쪽·오른쪽·위 순서의 대기·걷기 동작을 구성
- NPC 프롬프트 요약: 플레이어와 같은 화풍으로 안내자, 씨앗 상점 주인, 노년 주민과 젊은 주민의 4방향 모습을 구성
- UI 프롬프트 요약: 글자가 없는 가로형 대화창과 안내창을 중심으로 초상·이름표·본문·아이콘·버튼 힌트 영역을 구성하고 9-slice에 적합한 형태를 지향
- 마을 프롬프트 요약: 같은 건물과 출입구 조건을 유지한 채 광장 중심 순환형, 강변 자연형, 중심가 일자형의 세 가지 탑다운 배치를 비교

이미지 생성 결과는 논리적 32×32 셀 방향을 보여주는 고해상도 콘셉트다. 그대로 32×32 단위로 슬라이스할 수 있는 최종 스프라이트 시트는 아니다.
