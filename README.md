# 🎙️ Teleprompter CLI (.NET 8)

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Terminal](https://img.shields.io/badge/Terminal-ANSI%20VT100-black?logo=windows-terminal)](https://learn.microsoft.com/windows/terminal/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

> **Teleprompter de alta performance para linha de comando, desenvolvido em C# e .NET 8.**
> Projetado para criadores de conteúdo, palestrantes e videomakers que precisam de uma leitura fluida, ritmo natural e zero distrações durante gravações de vídeos, podcasts ou apresentações ao vivo.

---

## 📑 Sumário

- [Visão Geral](#-visão-geral)
- [Demonstração Visual (Prints)](#-demonstração-visual-prints)
- [Recursos Principais](#-recursos-principais)
- [Dependências e Requisitos](#-dependências-e-requisitos)
- [Instalação e Compilação](#-instalação-e-compilação)
- [Como Usar (Comandos)](#-como-usar-comandos)
- [Parâmetros de Linha de Comando](#-parâmetros-de-linha-de-comando)
- [Atalhos e Controles em Tempo Real](#-atalhos-e-controles-em-tempo-real)
- [Sintaxe de Roteiros e Marcações (Cues)](#-sintaxe-de-roteiros-e-marcações-cues)
- [Dicas de Gravação para Criadores](#-dicas-de-gravação-para-criadores)
- [Estrutura da Arquitetura](#-estrutura-da-arquitetura)
- [Licença](#-licença)

---

## 🌟 Visão Geral

Ao gravar vídeos falando para a câmera, a maioria dos teleprompters tradicionais força o olhar a rolar continuamente de baixo para cima, o que causa desvio evidente do contato visual ou sensação de fala robótica.

O **Teleprompter CLI** resolve isso com:
1. **Linha de Foco Fixa (*Eye-Line*)**: Seus olhos repousam sempre na mesma altura central da tela, enquanto as frases transitam suavemente.
2. **Segmentação por Respiração**: O texto é dividido em sentenças e ideias completas, evitando quebras no meio de palavras ou expressões.
3. **Cálculo Real de PPM (Palavras Por Minuto)**: O tempo de exposição de cada frase é proporcional à sua quantidade de palavras e pontuações (`...`, `!`, `?`).
4. **Instruções de Gravação (*Cues*)**: Marcações cênicas como `[Olhar para a câmera]` ou `[Pausa]` recebem destaque visual e pausas cronometradas.

---

## 🖥️ Demonstração Visual (Prints)

### 1. Tela de Leitura em Tempo Real (Eye-Line e HUD)

```text
╔════════════════════ TELEPROMPTER CLI  |  exemplo_roteiro.txt ════════════════════╗
║ ▶ EM REPRODUÇÃO │ Frase: 4/15 (27%) │ Velocidade: 130 PPM │ Tempo: 00:08         ║
║ [Espaço] Pausar/Retomar │ [← / →] Frase │ [↑ / ↓] Velocidade │ [R] Reiniciar │ [Ctrl+C] Sair ║
║ Próxima frase em: 3.4s [████████████░░░░░░░░░░░░]                                ║
╠══════════════════════════════════════════════════════════════════════════════════╣
║        [CUE] [Sorrir e olhar fixamente para a lente da câmera]                   ║
║        Olá, pessoal!                                                             ║
║        Sejam muito bem-vindos a mais um vídeo do canal.                          ║
╠══ [LINHA DE FOCO] ═══════════════════════════════════════════════════════════════╣
║   ▶ Hoje nós vamos conversar sobre como criar apresentações muito mais naturais  ║
║     e profissionais...                                                           ║
╚══════════════════════════════════════════════════════════════════════════════════╝
║        usando um teleprompter diretamente no seu terminal!                       ║
║        [CUE] [Ênfase]                                                            ║
║        Muitas pessoas acham que usar teleprompter deixa a fala engessada...      ║
╚══════════════════════════════════════════════════════════════════════════════════╝
```

---

### 2. Destaque Automático de Instrução de Cena (*Cue*)

Quando há uma instrução de cena entre colchetes no roteiro (ex: `[Pausa]`, `[Tom reflexivo]`), a caixa muda de cor para dourado/amarelo com estrelas de marcação:

```text
╠══ [INSTRUÇÃO DE CENA] ═══════════════════════════════════════════════════════════╣
║   ★  [Sorrir e olhar fixamente para a lente da câmera]  ★                        ║
╚══════════════════════════════════════════════════════════════════════════════════╝
```

---

### 3. Modo de Teste e Diagnóstico (`--test`)

Permite inspecionar o roteiro, validar contagem de palavras, verificar o tempo total estimado de gravação e identificar marcações antes de entrar no modo teleprompter:

```text
> teleprompter-cli exemplo_roteiro.txt --test

Carregando roteiro: exemplo_roteiro.txt...

=== [DIAGNÓSTICO DO ROTEIRO - MODO TESTE] ===
Total de itens: 15
Marcações de cena (Cues): 5
Frases de fala: 10
Tempo total estimado (@ 130 PPM): 01:01

--- Primeiros 8 itens: ---
001. [CUE]  (2,0s | 9 palavras): [Sorrir e olhar fixamente para a lente da câmera]
002. [FALA] (2,1s | 2 palavras): Olá, pessoal!
003. [FALA] (4,2s | 9 palavras): Sejam muito bem-vindos a mais um vídeo do canal.
004. [FALA] (6,8s | 13 palavras): Hoje nós vamos conversar sobre como criar apresentações muito mais naturais e profissionais...
005. [FALA] (3,7s | 7 palavras): usando um teleprompter diretamente no seu terminal!
006. [CUE]  (1,8s | 1 palavras): [Ênfase]
007. [FALA] (5,5s | 12 palavras): Muitas pessoas acham que usar teleprompter deixa a fala engessada ou robótica.
008. [FALA] (5,5s | 12 palavras): Mas o grande segredo está no ritmo, nas pausas e na respiração.

--- Últimos 4 itens: ---
012. [CUE]  (2,0s | 2 palavras): [Tom reflexivo]
013. [FALA] (5,1s | 11 palavras): É leve, rápido e não consome recursos desnecessários da sua máquina.
014. [FALA] (7,4s | 15 palavras): Se você gostou dessa ideia, deixe o seu like e compartilhe com quem produz conteúdo!
015. [CUE]  (2,0s | 5 palavras): [Acenar e finalizar com sorriso]

Teste concluído com sucesso.
```

---

### 4. Modo Interativo (Arrastar e Soltar)

Se iniciado sem argumentos, o utilitário exibe um banner acolhedor e permite que você simplesmente arraste seu arquivo `.txt` para a janela:

```text
╔════════════════════════════════════════════════════════════════════╗
║                    TELEPROMPTER CLI (.NET 8)                       ║
╚════════════════════════════════════════════════════════════════════╝

Informe o caminho do arquivo .txt do roteiro
(Dica: você pode simplesmente arrastar o arquivo .txt para cá):
> 
```

---

## ✨ Recursos Principais

- **Zero Flicker (Sem tremores ou piscamentos de tela)**: Renderização seletiva com sequências VT100 / ANSI e controle de cursor. Durante a reprodução, apenas o HUD e a barra de progresso são repintados.
- **Histórico e Prévia em Níveis de Contraste**: As frases anteriores aparecem atenuadas em cinza escuro (*dimmed*), a frase atual brilha em branco no centro, e os próximos tópicos aparecem em cinza neutro.
- **Detecção Inteligente de Janela (Responsive)**: Ao redimensionar a janela do terminal, o teleprompter detecta a nova largura/altura e reajusta as quebras de linha instantaneamente.
- **Suporte UTF-8 / Caracteres Especiais**: Compatível com acentuação, cedilhas, travessões e aspas sem quebrar caracteres. Fallback automático para Latin-1 caso o arquivo antigo não use UTF-8.
- **Controle Total pelo Teclado**: Ajuste de velocidade em tempo real, pausas com a barra de espaço, retrocesso ou avanço rápido de frases.

---

## 📦 Dependências e Requisitos

### Requisitos do Sistema
- **Sistema Operacional**: Windows 10/11, Linux (Ubuntu, Debian, Fedora, etc.) ou macOS (12+).
- **Terminal Recomendado**:
  - Windows: [Windows Terminal](https://github.com/microsoft/terminal), PowerShell 7+, PowerShell 5.1 ou CMD com suporte a VT100.
  - Linux/macOS: Bash, Zsh, iTerm2, Kitty, Alacritty ou Gnome Terminal.
- **Codificação de Fonte**: Fonte monoespaçada com suporte a símbolos (ex: *Cascadia Code*, *Consolas*, *JetBrains Mono*, *Fira Code*).

### Dependências de Desenvolvimento / Execução
- **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** (necessário apenas para compilar a partir do código-fonte ou rodar via `dotnet run`).
- *Se você usar o binário pré-compilado autossuficiente (`self-contained`), nenhuma instalação externa do .NET é necessária.*

---

## 🛠️ Instalação e Compilação

### 1. Clonar o Repositório
```bash
git clone https://github.com/seu-usuario/teleprompter-cli.git
cd teleprompter-cli
```

### 2. Executar em Modo de Desenvolvimento
```bash
dotnet run -- exemplo_roteiro.txt
```

### 3. Compilar em Modo Release
```bash
dotnet build -c Release
```

### 4. Gerar Executável Único e Portátil (Standalone / Self-Contained)

Para distribuir o teleprompter para qualquer pessoa sem exigir que ela instale o .NET SDK:

#### No Windows (x64):
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```
> O arquivo gerado `publish/teleprompter-cli.exe` funcionará em qualquer PC Windows 10/11 com um duplo clique!

#### No Linux (x64):
```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

#### No macOS (Apple Silicon / arm64):
```bash
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

---

## 🚀 Como Usar (Comandos)

### Método 1: Linha de Comando Direta (Recomendado)
Passe o caminho do seu arquivo `.txt` como argumento:

```bash
# Via executável publicado
./teleprompter-cli "meu_roteiro.txt"

# Ou via .NET CLI
dotnet run -- "meu_roteiro.txt"
```

> **Nota:** Se o caminho contiver espaços ou caracteres especiais, envolva-o sempre entre aspas (`"..."`).

---

### Método 2: Modo Interativo (Arrastar e Soltar)
1. Abra o `teleprompter-cli.exe` com dois cliques (ou digite `teleprompter-cli` sem parâmetros no terminal).
2. Arraste o arquivo `.txt` do seu explorador de arquivos para dentro da janela do terminal.
3. Pressione `Enter`. O teleprompter carregará o roteiro imediatamente!

---

### Método 3: Modo de Diagnóstico / Validação (`--test`)
Ideal para verificar quanto tempo seu vídeo vai durar e conferir a divisão de frases antes de ligar a câmera:

```bash
teleprompter-cli "roteiro.txt" --test
```
*(ou `dotnet run -- "roteiro.txt" --test`)*

---

## ⚙️ Parâmetros de Linha de Comando

| Parâmetro | Tipo | Descrição | Exemplo |
| :--- | :--- | :--- | :--- |
| `<caminho_arquivo>` | Posicional (string) | Caminho relativo ou absoluto do arquivo de texto (`.txt`) com o roteiro. Suporta aspas. | `teleprompter-cli "C:\Roteiros\video.txt"` |
| `--test` | Flag (opcional) | Executa o teste de diagnóstico, exibindo totais de frases, marcações de cena, tempo estimado e amostras. Não inicia a reprodução visual. | `teleprompter-cli "video.txt" --test` |
| *(Nenhum)* | - | Quando chamado sem argumentos, abre a interface interativa com prompt para inserção ou arrasto do caminho do arquivo. | `teleprompter-cli` |

---

## ⌨️ Atalhos e Controles em Tempo Real

Durante a execução da tela de teleprompter, os seguintes atalhos estão disponíveis:

| Tecla | Função | Detalhes |
| :---: | :--- | :--- |
| **`Espaço`** | **Pausar / Retomar** | O teleprompter inicia **pausado** por padrão para você respirar e se posicionar. Pressione espaço para iniciar a rolagem. |
| **`←`** (Seta Esquerda) | **Voltar Frase** | Volta para a sentença anterior e reinicia o cronômetro da frase. Excelente para refazer uma fala errada! |
| **`→`** (Seta Direita) | **Avançar Frase** | Salta imediatamente para a próxima frase sem esperar o temporizador. |
| **`↑`** (Seta Cima) | **Aumentar Velocidade** | Adiciona **+10 PPM** (Palavras por Minuto). Limite máximo de 350 PPM. |
| **`↓`** (Seta Baixo) | **Diminuir Velocidade** | Reduz **-10 PPM**. Limite mínimo de 40 PPM. |
| **`R`** ou **`r`** | **Reiniciar do Início** | Volta para a primeira frase do roteiro e pausa a reprodução. |
| **`Esc`** / **`Q`** / **`Ctrl + C`** | **Encerrar** | Encerra a execução com segurança, restaurando as cores do terminal e o cursor visível. |

---

## 📝 Sintaxe de Roteiros e Marcações (Cues)

O parser do **Teleprompter CLI** foi pensado especificamente para a forma como criadores de conteúdo escrevem roteiros. Você pode escrever textos normais ou adicionar anotações cênicas.

### Exemplo de Roteiro (`exemplo_roteiro.txt`)
```text
[Sorrir e olhar fixamente para a lente da câmera]
Olá, pessoal! Sejam muito bem-vindos a mais um vídeo do canal.
Hoje nós vamos conversar sobre como criar apresentações muito mais naturais e profissionais... usando um teleprompter diretamente no seu terminal!

[Ênfase]
Muitas pessoas acham que usar teleprompter deixa a fala engessada ou robótica.
Mas o grande segredo está no ritmo, nas pausas e na respiração.

[Pausa dramática]
Quando você domina o tempo das frases, o texto simplesmente flui.
Você já testou gravar usando o terminal do seu computador?

[Tom reflexivo]
É leve, rápido e não consome recursos desnecessários da sua máquina.
Se você gostou dessa ideia, deixe o seu like e compartilhe com quem produz conteúdo!
[Acenar e finalizar com sorriso]
```

### Regras do Parser e Tempos Automáticos

1. **Separação por Parágrafos e Sentenças**:
   - Linhas em branco separam parágrafos.
   - O parser divide o texto automaticamente em pontuações de término de sentença (`.`, `!`, `?`, `…`).
2. **Marcações de Cena (*Cues*)**:
   - Qualquer texto envolvido por colchetes `[ ... ]` é reconhecido como instrução cênica.
   - Não se mistura com o texto da fala e recebe destaque visual em amarelo.
   - **Tempos predefinidos por palavra-chave no Cue**:
     - Contém `"pausa"` (ex: `[Pausa de 3s]`, `[Pausa dramática]`): **~2.8 segundos**.
     - Contém `"ênfase"` ou `"enfase"` (ex: `[Ênfase nesta palavra]`): **~1.8 segundos**.
     - Contém `"reflexivo"` (ex: `[Tom reflexivo]`): **~2.0 segundos**.
     - Outras marcações de cena genéricas: **2.0 segundos**.
3. **Compensação por Pontuação**:
   - Sentenças terminadas em reticências (`...` ou `…`) ganham **+0.8s** de tempo para pausa dramática/continuidade.
   - Sentenças terminadas em exclamação (`!`) ou interrogação (`?`) ganham **+0.5s** para entonação.
4. **Piso Mínimo de Leitura**:
   - Frases muito curtas têm tempo mínimo garantido de **1.6 segundos**, garantindo que nenhuma frase passe rápido demais pelos seus olhos.

---

## 💡 Dicas de Gravação para Criadores

- **Posicionamento do Terminal**:
  Coloque a janela do terminal no topo central do monitor, logo abaixo da sua webcam ou lente de câmera. Como a caixa de foco (*Eye-Line*) fica fixa na mesma linha, quem assiste ao vídeo terá a impressão de que você está olhando diretamente nos olhos do espectador.
- **Ajuste do Tamanho da Fonte**:
  Use `Ctrl` + `Rodinha do Mouse` (ou `Ctrl` + `+`) no Windows Terminal para aumentar o tamanho da fonte. Uma fonte maior permite que você se posicione a 1 ou 2 metros de distância do monitor mantendo leitura 100% confortável.
- **Velocidade Sugerida**:
  - **110 a 130 PPM**: Ritmo calmo, ótimo para tutoriais técnicos, aulas ou explicações complexas.
  - **130 a 150 PPM**: Ritmo dinâmico para vídeos de YouTube, reviews e vlogs informativos.
  - **150+ PPM**: Vídeos em formato *Shorts*, *Reels* ou *TikTok*.
- **Errou uma frase? Não pare a gravação!**:
  Apenas aperte a **Seta Esquerda (`←`)** no teclado. O teleprompter volta uma frase e você refaz a fala sem perder o fluxo de gravação.

---

## 🏛️ Estrutura da Arquitetura

O projeto é estruturado de forma desacoplada e limpa:

```text
teleprompter-cli/
├── Models/
│   └── PromptItem.cs          # Modelo de item (frase ou cue), contagem e cálculo de duração
├── Services/
│   ├── TextParser.cs          # Parser regex de roteiro, separação de sentenças e cues
│   └── TeleprompterEngine.cs  # Loop principal assíncrono (50ms), controle de estado e inputs
├── UI/
│   ├── ConsoleRenderer.cs     # Motor de desenho ANSI/VT100 (Eye-line, HUD, histórico e prévia)
│   └── TerminalHelper.cs      # Utilitários de console Windows VT100 e controle de cursor
├── Program.cs                 # Ponto de entrada (CLI args, drag-and-drop, modo teste)
├── teleprompter-cli.csproj    # Definição do projeto .NET 8
└── exemplo_roteiro.txt        # Roteiro de demonstração pronto para teste
```

---

## 📄 Licença

Distribuído sob a licença **MIT**. Consulte `LICENSE` para mais informações.

Desenvolvido para criadores de conteúdo que valorizam simplicidade, performance e controle no terminal. 🚀
