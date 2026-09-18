# Code signing policy

O projeto pretende utilizar **assinatura gratuita fornecida por SignPath.io, com certificado da SignPath Foundation**, após sua aprovação.

## Funções da equipe

- Committer e revisor: Lucas — Suporte TI.
- Aprovador das solicitações de assinatura: Lucas — Suporte TI.

Enquanto houver apenas um mantenedor, alterações externas deverão ser propostas por pull request e revisadas pelo mantenedor. Cada solicitação de assinatura exigirá aprovação manual.

## Origem e integridade

- O binário é produzido exclusivamente pelo workflow versionado neste repositório.
- O RustDesk oficial é baixado de sua página de releases, com versão e SHA-256 fixados.
- A compilação falha se o hash ou a assinatura Authenticode do componente oficial não forem válidos.
- Chaves privadas e certificados nunca são armazenados no repositório.

## Privacidade

Consulte [PRIVACY.md](PRIVACY.md). Este programa não transfere informações para outros sistemas além da infraestrutura de acesso remoto descrita, exceto quando solicitado pelo usuário ao iniciar e autorizar o atendimento.
