# Política de privacidade

O Suporte TI - Acesso Remoto não possui telemetria própria, não cria conta de usuário e não coleta arquivos por conta própria.

Ao ser aberto, o componente RustDesk se conecta à infraestrutura privada de acesso remoto da Suporte TI para obter um ID de sessão e permitir o estabelecimento de uma conexão. O atendimento somente pode ser iniciado quando o usuário fornece voluntariamente ao técnico o ID e a senha de uso único exibidos na tela.

O destino de rede configurado é `server.suporteti.info`, operado pela Suporte TI. O cliente utiliza as portas padrão do RustDesk Server OSS documentadas no README. A chave publicada no projeto é somente a chave pública usada para validar o servidor; nenhuma chave privada é distribuída.

Durante uma sessão autorizada, dados necessários ao funcionamento do acesso remoto — como imagem da tela, entradas de teclado e mouse e recursos que o usuário permitir — podem trafegar entre o computador atendido e o técnico, diretamente ou pelo relay privado. O comportamento e a política do componente RustDesk estão documentados no [projeto oficial](https://github.com/rustdesk/rustdesk).

O aplicativo não configura acesso desassistido, não instala serviço permanente e não mantém uma sessão depois que o usuário fecha a janela.

Contato sobre privacidade: `lucas@suporteti.info`.
