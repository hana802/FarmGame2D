# FarmGame2D 프로젝트 저장 현황

기준일: 2026-11-08

## 저장 위치

- Unity 프로젝트 원본: `C:\Users\User\Desktop\sud\FarmGame2D`
- 실행용 WebGL 빌드: `C:\Users\User\Desktop\sud\FarmGame2D\Builds\WebGL`
- 화면 검증 이미지: `C:\Users\User\Desktop\sud\FarmGame2D\Builds\UI-QA`
- 기획 및 작업 기록: `C:\Users\User\Desktop\sud\FarmGame2D\docs`
- 자동 검증 도구: `C:\Users\User\Desktop\sud\FarmGame2D\Tools`

## 현재 프로토타입에 반영된 내용

- 남성·여성 플레이어 선택과 2D 이동 애니메이션
- 플레이어가 들고 있는 도구와 씨앗 표시
- 농사 밭, 밀 재배, 계절별 씨앗과 인벤토리
- 닭·소와 축사, 판매 상자, 상점, 광석과 잡초
- 마을 건물, 광장, 연못, 나무, NPC 이동과 대화
- NPC 초상화, 하얀 말풍선, 대화창과 안내 문구
- 게임 시간, 낮·밤 밝기, 계절별 날씨 연출
- 퀘스트·휴대폰 UI·저장 및 불러오기 기반
- 건물 뒤로 이동할 때 캐릭터가 가려지는 깊이 처리
- 섬 경계 밖 파도 무늬 바다와 해안 이동 차단

## 최근 확인 완료 항목

- 광장 게시판과 닭장이 캐릭터보다 올바르게 앞에 표시됨
- 캐릭터가 건물 앞에 오면 다시 자연스럽게 표시됨
- 남쪽 해안에서 바다가 보이고 캐릭터가 경계를 넘지 못함
- Unity PlayMode 핵심 월드 테스트 통과
- 최신 WebGL 빌드 및 브라우저 화면 확인 완료

## 최신 실행 주소

로컬 서버가 실행 중일 때 아래 주소로 확인할 수 있습니다.

`http://127.0.0.1:8765/?build=structure-sea-final-20261108-1005`

## 참고

`Assets`, `Packages`, `ProjectSettings`가 Unity 프로젝트의 핵심 원본입니다. `Builds/WebGL`은 바로 실행해 볼 수 있는 결과물이며, `Library`와 `Logs`는 Unity가 다시 생성할 수 있는 작업용 폴더입니다.
