# Maca Rush - MVP Unity

Maca Rush e um prototipo cooperativo local para 4 jogadores. A equipe precisa carregar uma maca com um paciente ate a ambulancia, passando por corredor, rota de elevador/escada, rua e zona final.

## Versao indicada

- Unity 6000 LTS ou Unity 2022.3 LTS.
- Projeto 3D padrao.
- Sem assets pagos e sem backend.

## Estrutura

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

## Como montar a cena jogavel

1. Abra ou crie uma cena 3D vazia na Unity.
2. Crie um GameObject vazio chamado `SceneBuilder`.
3. Adicione o componente `MacaRushSceneBuilder`.
4. No menu de contexto do componente, execute `Build Prototype Scene`.
5. Pressione Play.

O builder cria automaticamente:

- 4 jogadores locais;
- maca fisica com 4 alcas;
- paciente com vida e estados;
- HUD;
- mapa com hospital, elevador/escada, rua e ambulancia;
- portas automaticas;
- obstaculos leves;
- buracos, cones, chuva escorregadia e carros;
- diretor de eventos aleatorios.

As tags `Player` e `Maca` sao opcionais. Se elas existirem, o builder usa; se nao existirem, os scripts funcionam por componentes.

## Controles

- Player 1: WASD, pegar/soltar `E`, correr `Left Shift`.
- Player 2: IJKL, pegar/soltar `U`, correr `Right Shift`.
- Player 3: TFGH, pegar/soltar `R`, correr `Y`.
- Player 4: setas, pegar/soltar `Right Control`, correr `Right Alt`.
- Reiniciar apos vitoria/derrota: `R`.

Cada jogador so pega a sua propria alca. A stamina cai ao correr e ao carregar a maca; com stamina baixa, o jogador perde eficiencia.

## Sistemas principais

- `PlayerCarryController`: movimento, corrida, stamina, pegar/soltar alca e empurrar objetos leves.
- `MacaHandle`: junta elastica entre player e maca.
- `MacaStretcher`: peso, centro de massa, dano por impacto, dano por inclinacao, derrota por tombamento e queda.
- `PatientHealth`: vida, estados `Stable`, `Injured`, `Critical`, `Dying`, `Dead` e cor simples do paciente.
- `GameManager`: tempo de partida, objetivo, vitoria, derrota e reinicio.
- `SimpleHud`: vida, estado, tempo, objetivo, alerta critico e mensagens finais.
- `RandomEventDirector`: luz piscando, porta travando, paciente se mexendo, obstaculo atravessando, chao escorregadio e sirene visual.

## Condicoes de jogo

Vitoria:

- A maca entra na zona da ambulancia com o paciente vivo.

Derrota:

- Paciente morre.
- Maca fica virada por tempo demais.
- Paciente cai da maca.
- Paciente cai fora do mapa.
- Tempo maximo acaba.

## Problemas conhecidos

- O mapa e todo feito com primitivas da Unity; nao ha polimento visual.
- A sirene ainda e feedback visual se nenhum `AudioClip` for configurado no `RandomEventDirector`.
- O paciente ainda e um collider filho da maca; a "queda" e uma condicao de gameplay por inclinacao/altura, nao uma animacao ragdoll.
- Os valores de massa, dano e stamina provavelmente precisam de ajuste depois de testar com quatro pessoas.

## Proximos passos recomendados

1. Testar 4 jogadores no mesmo teclado e ajustar keys se houver ghosting.
2. Criar uma cena `.unity` salva em `Assets/MacaRush/Scenes`.
3. Transformar os objetos gerados em prefabs quando o layout estabilizar.
4. Migrar input para Unity Input System se for usar controles/gamepads.
5. Adicionar audio simples para sirene, batidas e paciente critico.
