# Suporte TI - Acesso Remoto

Cliente portátil de suporte remoto para Windows, preparado para atendimentos autorizados da **Suporte TI**. O aplicativo apresenta somente o ID temporário e a senha de uso único que o cliente deve informar ao técnico.

O projeto utiliza o cliente oficial [RustDesk](https://github.com/rustdesk/rustdesk) 1.4.9, sem modificar seu binário, configurado para a infraestrutura privada da Suporte TI. O download do RustDesk é validado por SHA-256 e por sua assinatura Authenticode antes da compilação.

Configuração de rede incorporada e auditável:

- Servidor ID/relay: `server.suporteti.info`
- Chave pública do servidor: `2Qg2xgikAAFFM8cP3l4LZm9nuOIKbWrWgCSXvfVY1zs=`
- Portas utilizadas pelo RustDesk Server OSS: TCP 21115–21117 e UDP 21116

## Segurança e consentimento

- O acesso só ocorre quando o usuário abre o aplicativo e fornece voluntariamente o ID e a senha temporária ao técnico.
- A senha é gerada pelo RustDesk e muda ao reiniciar o cliente.
- O aplicativo não instala serviço permanente, não cria acesso desassistido e não altera configurações do Windows.
- O inicializador encerra somente o processo que ele próprio iniciou ao fechar sua janela.
- O tráfego utiliza o servidor privado configurado da Suporte TI.
- O aplicativo não contém senha de acesso desassistido, chave privada ou credencial administrativa.

## Compilar

Requisitos: Windows 10/11, Windows PowerShell 5.1 e .NET Framework 4.x.

```powershell
./scripts/build.ps1
```

O artefato será criado em `artifacts/SuporteTI-AcessoRemoto.exe`.

## Componentes e licenças

- Inicializador Suporte TI: AGPL-3.0-or-later.
- RustDesk 1.4.9: AGPL-3.0, distribuído sem modificação. Consulte [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

## Privacidade

Consulte [PRIVACY.md](PRIVACY.md). O software não envia telemetria própria.

## Code signing policy

Consulte [CODE_SIGNING_POLICY.md](CODE_SIGNING_POLICY.md). O objetivo é usar assinatura gratuita fornecida por SignPath.io, com certificado da SignPath Foundation, após aprovação do projeto.

## Suporte e segurança

Problemas de segurança devem ser comunicados conforme [SECURITY.md](SECURITY.md).
