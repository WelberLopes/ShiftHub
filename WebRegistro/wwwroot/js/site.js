document.addEventListener('DOMContentLoaded', function () {
    // Seletores dos elementos principais
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebarBackdrop = document.getElementById('sidebarBackdrop');
    const appContainer = document.querySelector('.app-container'); // Alvo principal

    const sidebarStateKey = 'sidebarDesktopState'; // Chave para salvar o estado
    const mobileBreakpoint = 992; // Largura para considerar 'mobile'

    // Função principal que trata o clique no botão de toggle
    function handleToggle() {
        if (!appContainer) return; // Segurança: se o container não existir, não faz nada

        if (window.innerWidth < mobileBreakpoint) {
            // Comportamento Mobile: Adiciona/remove a classe para o menu off-canvas
            appContainer.classList.toggle('sidebar-open');
        } else {
            // Comportamento Desktop: Adiciona/remove a classe para o menu recolhido
            appContainer.classList.toggle('sidebar-collapsed');

            // Salva o estado (recolhido ou expandido) no localStorage para o desktop
            const isCollapsed = appContainer.classList.contains('sidebar-collapsed');
            localStorage.setItem(sidebarStateKey, isCollapsed ? 'collapsed' : 'expanded');
        }
    }

    // Configuração inicial da sidebar quando a página carrega
    function setupSidebar() {
        if (!appContainer) return;

        if (window.innerWidth >= mobileBreakpoint) {
            // No Desktop, verifica se o estado salvo era 'recolhido' e aplica a classe
            if (localStorage.getItem(sidebarStateKey) === 'collapsed') {
                appContainer.classList.add('sidebar-collapsed');
            }
        }
    }

    // Adiciona o evento de clique ao botão do menu
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', handleToggle);
    }

    // Adiciona o evento de clique ao fundo escuro (backdrop) para fechar o menu mobile
    if (sidebarBackdrop) {
        sidebarBackdrop.addEventListener('click', () => {
            if (appContainer.classList.contains('sidebar-open')) {
                appContainer.classList.remove('sidebar-open');
            }
        });
    }

    // Executa a configuração inicial ao carregar a página
    setupSidebar();
});