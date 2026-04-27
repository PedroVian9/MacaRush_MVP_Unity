# Maca Rush - MVP Unity

Maca Rush e um prototipo cooperativo local para 4 jogadores. O objetivo e levar a maca com o paciente ate a ambulancia sem matar o paciente no caminho.

## 1. Requisitos

- Unity Hub instalado.
- Unity Editor `6000 LTS` ou `2022.3 LTS`.
- Projeto 3D (Built-in/URP simples; este MVP usa primitivas e scripts).
- Teclado para 4 jogadores locais.

## 2. Estrutura esperada

```text
Assets/
  MacaRush/
    Scripts/
      Core/
      Player/
      Stretcher/
      Patient/
      UI/
      Environment/
      Events/
    Prefabs/
      Player/
      Stretcher/
      Environment/
      UI/
    Scenes/
    Materials/
    Audio/
```

## 3. Criar o projeto do zero

1. Abra o Unity Hub.
2. Clique em `New project`.
3. Escolha template `3D`.
4. Nomeie como `MacaRush_MVP_Unity`.
5. Clique em `Create project`.

## 4. Importar este MVP

1. Feche o Unity Editor (recomendado para evitar conflito de reimport).
2. Copie a pasta `Assets/MacaRush` deste repositorio para dentro do seu projeto.
3. Reabra o projeto no Unity.
4. Aguarde o Unity terminar o `Importing...`.

Se o seu projeto ja tinha arquivos com o mesmo nome, mantenha os scripts mais novos deste MVP.

## 5. Montar a cena jogavel automatica

1. No Unity, abra uma cena vazia:
   - `File > New Scene` (Basic/Empty).
2. Salve a cena:
   - `File > Save As...`
   - caminho sugerido: `Assets/MacaRush/Scenes/PrototypeScene.unity`.
3. Crie um objeto vazio:
   - `Hierarchy > Create Empty`
   - renomeie para `SceneBuilder`.
4. Com `SceneBuilder` selecionado, clique `Add Component`.
5. Adicione `MacaRushSceneBuilder`.
6. No componente, clique no menu de contexto (3 pontinhos) e rode:
   - `Build Prototype Scene`.
7. A cena sera gerada automaticamente com:
   - 4 players;
   - maca + paciente;
   - HUD;
   - hospital, elevador/escada, rua e ambulancia;
   - obstaculos e eventos.

## 6. Executar (Play)

1. Clique em `Play`.
2. Teste os controles:
   - P1: `WASD`, pegar/soltar `E`, correr `Left Shift`.
   - P2: `IJKL`, pegar/soltar `U`, correr `Right Shift`.
   - P3: `TFGH`, pegar/soltar `R`, correr `Y`.
   - P4: `Setas`, pegar/soltar `Right Control`, correr `Right Alt`.
   - Reiniciar apos vitoria/derrota: `R`.
3. Objetivo: atravessar o mapa e entrar com a maca na zona da ambulancia.

## 7. O que validar no primeiro teste

1. Todos os 4 jogadores se movem.
2. Cada jogador so pega sua propria alca.
3. A maca inclina e pode tombar.
4. Vida do paciente cai com tempo/impacto/inclinacao.
5. Eventos aleatorios acontecem (luzes, portas, sirene, etc.).
6. HUD mostra estado, tempo, objetivo e pressao da partida.
7. Vitoria/derrota funcionam.

## 8. Configuracoes mais importantes no Inspector

Use estes componentes para balancear rapido:

- `GameManager`:
  - `maxMatchTime`
  - `difficultyByProgress` (curva de dificuldade)
- `PatientHealth`:
  - `passiveDrainPerSecond`
  - thresholds (`injuredThreshold`, `criticalThreshold`, `dyingThreshold`)
- `MacaStretcher`:
  - `mass`, `centerOfMassOffset`
  - `impactDamageMultiplier`
  - `tiltDamagePerSecond`
- `RandomEventDirector`:
  - `minDelay/maxDelay`
  - `minDelayAtMaxDifficulty/maxDelayAtMaxDifficulty`
  - toggles de eventos

## 9. Condicoes de fim de partida

Vitoria:

- Maca entra na zona da ambulancia com paciente vivo.

Derrota:

- Paciente morre.
- Maca fica virada por tempo demais.
- Paciente cai da maca.
- Paciente cai fora do mapa.
- Tempo maximo acaba.

## 10. Build para executavel (opcional)

1. Abra `File > Build Settings`.
2. Clique `Add Open Scenes`.
3. Escolha plataforma (ex.: Windows).
4. Clique `Build` ou `Build and Run`.

## 11. Problemas comuns e como resolver

### Cena vazia ao apertar Play

- Rode novamente `Build Prototype Scene` no `MacaRushSceneBuilder`.

### Script nao aparece no Add Component

- Verifique erros no `Console`; se houver erro de compilacao, a Unity nao carrega scripts novos.

### Controles de algum player nao respondem

- Teclados comuns podem ter `ghosting` com muitas teclas simultaneas.
- Reconfigure as teclas dos players nos componentes `PlayerCarryController`.

### Eventos nao aparecem

- Confira `eventsEnabled` no `RandomEventDirector`.

### Sirene sem som

- O visual funciona sem audio.
- Para som, adicione um `AudioSource` com `AudioClip` no `RandomEventDirector`.

## 12. Estado atual do prototipo

- Ja existe mapa completo com progresso de inicio ao fim.
- Ja existem obstaculos fixos e moveis.
- Ja existe dificuldade crescente ao longo da partida.
- Ainda precisa de polimento visual e ajuste fino de balanceamento.
