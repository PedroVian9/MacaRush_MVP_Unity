# Maca Rush - MVP Unity

Maca Rush agora esta em formato single-player: voce controla um personagem em terceira pessoa e empurra a maca ate a ambulancia, lidando com obstaculos e eventos.

## 1. Requisitos

- Unity Hub instalado.
- Unity Editor `6000 LTS` ou `2022.3 LTS`.
- Projeto 3D.
- Pacote `Unity UI (com.unity.ugui)` instalado no projeto.

## 2. Criar e importar

1. Crie um projeto `3D` no Unity Hub.
2. Copie a pasta `Assets/MacaRush` para dentro do projeto.
3. Abra o projeto e aguarde o import.

## 3. Montar a cena

1. Abra uma cena vazia e salve em `Assets/MacaRush/Scenes/PrototypeScene.unity`.
2. Crie um objeto vazio chamado `SceneBuilder`.
3. Adicione o componente `MacaRushSceneBuilder`.
4. No Inspector, mantenha `autoBuildOnPlay` ligado e `showMainMenuOnPlay` ligado.
5. Pressione `Play`.
6. No menu principal:
   - selecione o mapa no dropdown;
   - clique `Jogar`.

Se preferir pular o menu:

1. Desligue `showMainMenuOnPlay`.
2. Ajuste `defaultMapPreset`.
3. Pressione `Play` (ou execute `Build Prototype Scene` no contexto do componente).

## 4. Controles (single-player)

- Mover: `WASD`
- Correr: `Left Shift`
- Segurar/soltar maca: `E`
- Camera: mouse (`Mouse X` / `Mouse Y`)
- Reiniciar apos vitoria/derrota: `R`

## 5. Modos de mapa

- `Mapa Completo`: hospital -> elevador/escada -> rua -> ambulancia.
- `Hospital Sprint`: trecho curto no hospital com zona de embarque.
- `Street Dash`: foco em rua com trafego e hazards.

## 6. O que a cena gera automaticamente

- 1 jogador em terceira pessoa com blocagem detalhada.
- 1 maca com frame, rodas e paciente com risco real.
- HUD com vida/estado/tempo/objetivo.
- Cenario modular com sinalizacao por funcao (interacao, perigo, objetivo).
- Obstaculos fixos/moveis e eventos aleatorios configuraveis.
- FX leves de impacto, escorregao, alerta critico e sirene visual.

## 7. Objetivo e fim de partida

Vitoria:

- Empurrar a maca ate a zona final da ambulancia com o paciente vivo.

Derrota:

- Paciente morre.
- Maca fica virada por tempo demais.
- Paciente cai da maca.
- Paciente cai fora do mapa.
- Tempo maximo acaba.

## 8. Balanceamento principal (Inspector)

- `GameManager`: `maxMatchTime`, `difficultyByProgress`.
- `PatientHealth`: `passiveDrainPerSecond`.
- `MacaStretcher`: `mass`, `impactDamageMultiplier`, `tiltDamagePerSecond`.
- `SimpleFollowCamera`: distancia, sensibilidade, colisao e framing.
- `RandomEventDirector`: delays, intensidade e toggles de eventos.
- `MacaRushSceneBuilder`: `defaultMapPreset`, `autoBuildOnPlay`, `showMainMenuOnPlay`.

## 9. Troubleshooting rapido

Se aparecer erro `UnityEngine.UI`:

1. Abra `Window > Package Manager`.
2. Selecione `Unity Registry`.
3. Instale `Unity UI (com.unity.ugui)`.

Se a cena abrir vazia:

- Rode novamente `Build Prototype Scene`.

Se o menu nao aparecer:

- Verifique se existe um `MacaRushSceneBuilder` ativo na cena.
- Confirme `autoBuildOnPlay = true` e `showMainMenuOnPlay = true`.
