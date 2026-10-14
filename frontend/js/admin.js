const agendaDataInput = document.getElementById("agenda-data");
const agendaProfissionalSelect =
    document.getElementById("agenda-profissional");

const agendaForm = document.getElementById("form-agenda");
const agendaFeedback = document.getElementById("agenda-feedback");
const agendaTbody = document.getElementById("agenda-tbody");
const agendaDataTitulo = document.getElementById("agenda-data-titulo");

let agendaAtual = [];

// Comunicação HTTP com a API.

async function requisicaoApi(endpoint, options = {}) {
    const headers = { ...(options.headers ?? {}) };

    if (options.body) {
        headers["Content-Type"] = "application/json; charset=utf-8";
    }

    const response = await fetch(endpoint, {
        ...options,
        headers
    });

    if (response.status === 204) {
        return null;
    }

    const texto = await response.text();
    let dados = null;

    if (texto) {
        try {
            dados = JSON.parse(texto);
        } catch {
            dados = { detail: texto };
        }
    }

    if (!response.ok) {
        const errosValidacao = dados?.errors
            ? Object.values(dados.errors).flat().join(" ")
            : "";

        throw new Error(
            dados?.detail ||
            errosValidacao ||
            dados?.title ||
            `Erro HTTP ${response.status}.`
        );
    }

    return dados;
}

function mostrarMensagem(mensagem, tipo = "info") {
    agendaFeedback.textContent = mensagem;
    agendaFeedback.className = `admin-feedback admin-feedback--${tipo}`;
}

function limparMensagem() {
    agendaFeedback.textContent = "";
    agendaFeedback.className = "admin-feedback";
}

// Manipulação de datas locais.

function obterDataLocal(date = new Date()) {
    const ano = date.getFullYear();
    const mes = String(date.getMonth() + 1).padStart(2, "0");
    const dia = String(date.getDate()).padStart(2, "0");

    return `${ano}-${mes}-${dia}`;
}

function deslocarData(valor, quantidadeDias) {
    const [ano, mes, dia] = valor.split("-").map(Number);
    const data = new Date(ano, mes - 1, dia);
    data.setDate(data.getDate() + quantidadeDias);

    return obterDataLocal(data);
}

function formatarData(valor) {
    const [ano, mes, dia] = valor.split("-").map(Number);

    return new Date(ano, mes - 1, dia).toLocaleDateString("pt-BR", {
        weekday: "long",
        day: "2-digit",
        month: "long",
        year: "numeric"
    });
}

function formatarHora(valor) {
    const data = new Date(valor);

    if (Number.isNaN(data.getTime())) {
        return String(valor).slice(11, 16);
    }

    return data.toLocaleTimeString("pt-BR", {
        hour: "2-digit",
        minute: "2-digit"
    });
}

// Carregamento dos profissionais e resumo dos cadastros.

async function carregarProfissionais() {
    const profissionais = await requisicaoApi("/api/profissionais");

    agendaProfissionalSelect.replaceChildren();

    const todos = document.createElement("option");
    todos.value = "";
    todos.textContent = "Todos os profissionais";
    agendaProfissionalSelect.appendChild(todos);

    for (const profissional of profissionais) {
        const opcao = document.createElement("option");
        opcao.value = String(profissional.id);

        opcao.textContent = profissional.ativo
            ? `${profissional.nome} — ${profissional.especialidade}`
            : `${profissional.nome} — inativo`;

        agendaProfissionalSelect.appendChild(opcao);
    }
}



function renderizarTabelaVazia(tbody, colunas, mensagem) {
    const linha = document.createElement("tr");
    const celula = document.createElement("td");

    celula.colSpan = colunas;
    celula.className = "table-message";
    celula.textContent = mensagem;

    linha.appendChild(celula);
    tbody.replaceChildren(linha);
}

function renderizarTabelaPacientes(pacientes) {
    const tbody = document.getElementById("admin-pacientes-tbody");
    tbody.replaceChildren();

    if (pacientes.length === 0) {
        renderizarTabelaVazia(
            tbody,
            3,
            "Nenhum paciente cadastrado."
        );
        return;
    }

    pacientes.forEach((paciente) => {
        const linha = document.createElement("tr");

        linha.appendChild(criarCelula(paciente.nome));
        linha.appendChild(criarCelula(paciente.telefone));
        linha.appendChild(criarCelula(paciente.email || "—"));

        tbody.appendChild(linha);
    });
}

function renderizarTabelaProfissionais(profissionais) {
    const tbody = document.getElementById("admin-profissionais-tbody");
    tbody.replaceChildren();

    if (profissionais.length === 0) {
        renderizarTabelaVazia(
            tbody,
            3,
            "Nenhum profissional cadastrado."
        );
        return;
    }

    profissionais.forEach((profissional) => {
        const linha = document.createElement("tr");

        linha.appendChild(criarCelula(profissional.nome));
        linha.appendChild(criarCelula(profissional.especialidade));
        linha.appendChild(
            criarCelula(profissional.ativo ? "Ativo" : "Inativo")
        );

        tbody.appendChild(linha);
    });
}

function renderizarTabelaServicos(servicos) {
    const tbody = document.getElementById("admin-servicos-tbody");
    tbody.replaceChildren();

    if (servicos.length === 0) {
        renderizarTabelaVazia(
            tbody,
            3,
            "Nenhum serviço ativo cadastrado."
        );
        return;
    }

    servicos.forEach((servico) => {
        const linha = document.createElement("tr");

        linha.appendChild(criarCelula(servico.nome));
        linha.appendChild(criarCelula(servico.descricao || "—"));
        linha.appendChild(
            criarCelula(`${servico.duracaoMinutos} minutos`)
        );

        tbody.appendChild(linha);
    });
}

async function carregarResumoCadastros() {
    const feedback = document.getElementById("admin-listas-feedback");

    try {
        const [pacientes, profissionais, servicos] = await Promise.all([
            requisicaoApi("/api/pacientes"),
            requisicaoApi("/api/profissionais"),
            requisicaoApi("/api/servicos")
        ]);

        document.getElementById("resumo-pacientes").textContent =
            pacientes.length;

        document.getElementById("resumo-profissionais").textContent =
            profissionais.length;

        document.getElementById("resumo-servicos").textContent =
            servicos.length;

        renderizarTabelaPacientes(pacientes);
        renderizarTabelaProfissionais(profissionais);
        renderizarTabelaServicos(servicos);

        if (feedback) {
            feedback.textContent = "";
            feedback.className = "admin-feedback";
        }
    } catch (erro) {
        console.error("Falha ao carregar os cadastros:", erro);

        if (feedback) {
            feedback.textContent =
                `Não foi possível atualizar os cadastros: ${erro.message}`;

            feedback.className = "admin-feedback admin-feedback--error";
        }
    }
}


// Resumo numérico da agenda consultada.

function atualizarIndicadores(agendamentos) {
    const totalAgendados = agendamentos.filter(
        (a) => a.status === "Agendado"
    ).length;

    const totalConcluidos = agendamentos.filter(
        (a) => a.status === "Concluido"
    ).length;

    const totalCancelados = agendamentos.filter(
        (a) => a.status === "Cancelado"
    ).length;

    document.getElementById("total-atendimentos").textContent =
        agendamentos.length;

    document.getElementById("total-agendados").textContent =
        totalAgendados;

    document.getElementById("total-concluidos").textContent =
        totalConcluidos;

    document.getElementById("total-cancelados").textContent =
        totalCancelados;
}

// Renderização segura da tabela.

function criarCelula(texto, classe = "") {
    const celula = document.createElement("td");
    celula.textContent = texto;

    if (classe) {
        celula.className = classe;
    }

    return celula;
}

function renderizarAgenda(agendamentos) {
    agendaTbody.replaceChildren();

    if (agendamentos.length === 0) {
        const linha = document.createElement("tr");
        const celula = document.createElement("td");

        celula.colSpan = 7;
        celula.className = "table-message";
        celula.textContent = "Nenhum atendimento encontrado para esta data.";

        linha.appendChild(celula);
        agendaTbody.appendChild(linha);

        atualizarIndicadores([]);
        return;
    }

    agendamentos.forEach((agendamento) => {
        const linha = document.createElement("tr");

        linha.appendChild(
            criarCelula(
                `${formatarHora(agendamento.inicio)}–${formatarHora(agendamento.fim)}`,
                "appointment-time"
            )
        );

        linha.appendChild(
            criarCelula(agendamento.paciente, "appointment-patient")
        );

        linha.appendChild(
            criarCelula(agendamento.telefone, "appointment-phone")
        );

        linha.appendChild(criarCelula(agendamento.servico));
        linha.appendChild(criarCelula(agendamento.profissional));

        const celulaStatus = document.createElement("td");
        const status = document.createElement("span");
        const classeStatus = String(agendamento.status).toLowerCase();

        status.className = `status-label status-label--${classeStatus}`;
        status.textContent = agendamento.status;

        celulaStatus.appendChild(status);
        linha.appendChild(celulaStatus);

        const celulaAcoes = document.createElement("td");

        if (agendamento.status === "Agendado") {
            const botao = document.createElement("button");

            botao.type = "button";
            botao.className = "button button-danger button-small";
            botao.textContent = "Cancelar";

            botao.addEventListener("click", () => {
                cancelarAgendamento(agendamento, botao);
            });

            celulaAcoes.appendChild(botao);
        } else {
            celulaAcoes.textContent = "—";
        }

        linha.appendChild(celulaAcoes);
        agendaTbody.appendChild(linha);
    });

    atualizarIndicadores(agendamentos);
}

// Consulta da agenda diária.

async function carregarAgenda() {
    const data = agendaDataInput.value;

    if (!data) {
        mostrarMensagem("Selecione uma data para consultar a agenda.", "error");
        return;
    }

    limparMensagem();
    mostrarMensagem("Carregando atendimentos...", "info");

    agendaTbody.replaceChildren();

    const parametros = new URLSearchParams({ data });

    if (agendaProfissionalSelect.value) {
        parametros.set(
            "profissionalId",
            agendaProfissionalSelect.value
        );
    }

    try {
        const agendamentos = await requisicaoApi(
            `/api/agendamentos/agenda?${parametros.toString()}`
        );

        agendaAtual = agendamentos;
        agendaDataTitulo.textContent = formatarData(data);

        renderizarAgenda(agendaAtual);

        mostrarMensagem(
            `${agendaAtual.length} atendimento(s) encontrado(s).`,
            "success"
        );
    } catch (erro) {
        agendaAtual = [];
        renderizarAgenda([]);

        mostrarMensagem(
            `Não foi possível carregar a agenda: ${erro.message}`,
            "error"
        );
    }
}

// Cancelamento pela interface administrativa.

async function cancelarAgendamento(agendamento, botao) {
    const confirmado = window.confirm(
        `Deseja cancelar o agendamento #${agendamento.id} de ${agendamento.paciente}?`
    );

    if (!confirmado) {
        return;
    }

    botao.disabled = true;
    botao.textContent = "Cancelando...";

    try {
        await requisicaoApi(
            `/api/agendamentos/${agendamento.id}/cancelar`,
            {
                method: "POST",
                body: JSON.stringify({
                    telefone: agendamento.telefone
                })
            }
        );

        await carregarAgenda();

        mostrarMensagem(
            `Agendamento #${agendamento.id} cancelado.`,
            "success"
        );
    } catch (erro) {
        mostrarMensagem(
            `Não foi possível cancelar: ${erro.message}`,
            "error"
        );

        botao.disabled = false;
        botao.textContent = "Cancelar";
    }
}

// Eventos da interface.

agendaForm.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    await carregarAgenda();
});

document.getElementById("agenda-hoje").addEventListener("click", async () => {
    agendaDataInput.value = obterDataLocal();
    await carregarAgenda();
});

document.getElementById("agenda-anterior").addEventListener(
    "click",
    async () => {
        if (!agendaDataInput.value) {
            agendaDataInput.value = obterDataLocal();
        }

        agendaDataInput.value = deslocarData(
            agendaDataInput.value,
            -1
        );

        await carregarAgenda();
    }
);

document.getElementById("agenda-proximo").addEventListener(
    "click",
    async () => {
        if (!agendaDataInput.value) {
            agendaDataInput.value = obterDataLocal();
        }

        agendaDataInput.value = deslocarData(
            agendaDataInput.value,
            1
        );

        await carregarAgenda();
    }
);

function mostrarFeedbackCadastro(id, mensagem, tipo = "success") {
    const elemento = document.getElementById(id);

    elemento.textContent = mensagem;
    elemento.className = `feedback feedback--${tipo}`;
}

async function executarCadastro({
    formId,
    feedbackId,
    endpoint,
    obterDados,
    mensagemSucesso,
    atualizarProfissionais = false
}) {
    const formulario = document.getElementById(formId);
    const botao = formulario.querySelector('button[type="submit"]');
    const textoOriginal = botao.textContent;

    botao.disabled = true;
    botao.textContent = "Salvando...";

    try {
        await requisicaoApi(endpoint, {
            method: "POST",
            body: JSON.stringify(obterDados(formulario))
        });

        formulario.reset();

        mostrarFeedbackCadastro(
            feedbackId,
            mensagemSucesso,
            "success"
        );

        await carregarResumoCadastros();

        if (atualizarProfissionais) {
            await carregarProfissionais();
        }

        await carregarOpcoesGerenciamento();

    } catch (erro) {
        mostrarFeedbackCadastro(
            feedbackId,
            erro.message,
            "error"
        );
    } finally {
        botao.disabled = false;
        botao.textContent = textoOriginal;
    }
}

document.getElementById("admin-form-paciente")
    .addEventListener("submit", async (evento) => {
        evento.preventDefault();

        await executarCadastro({
            formId: "admin-form-paciente",
            feedbackId: "admin-paciente-feedback",
            endpoint: "/api/pacientes",
            obterDados: () => ({
                nome: document.getElementById("admin-paciente-nome").value.trim(),
                telefone: document.getElementById("admin-paciente-telefone").value.trim(),
                email: document.getElementById("admin-paciente-email").value.trim() || null
            }),
            mensagemSucesso: "Paciente cadastrado com sucesso."
        });
    });

document.getElementById("admin-form-profissional")
    .addEventListener("submit", async (evento) => {
        evento.preventDefault();

        await executarCadastro({
            formId: "admin-form-profissional",
            feedbackId: "admin-profissional-feedback",
            endpoint: "/api/profissionais",
            obterDados: () => ({
                nome: document.getElementById("admin-profissional-nome").value.trim(),
                especialidade: document.getElementById("admin-profissional-especialidade").value.trim()
            }),
            mensagemSucesso: "Profissional cadastrado com sucesso.",
            atualizarProfissionais: true
        });
    });

document.getElementById("admin-form-servico")
    .addEventListener("submit", async (evento) => {
        evento.preventDefault();

        await executarCadastro({
            formId: "admin-form-servico",
            feedbackId: "admin-servico-feedback",
            endpoint: "/api/servicos",
            obterDados: () => ({
                nome: document.getElementById("admin-servico-nome").value.trim(),
                descricao: document.getElementById("admin-servico-descricao").value.trim() || null,
                duracaoMinutos: Number(document.getElementById("admin-servico-duracao").value)
            }),
            mensagemSucesso: "Serviço cadastrado com sucesso."
        });
    });

document.getElementById("admin-atualizar-cadastros")
    .addEventListener("click", async () => {
        try {
            await Promise.all([
                carregarResumoCadastros(),
                carregarProfissionais(),
                carregarOpcoesGerenciamento()
            ]);

            await carregarAgenda();

            mostrarMensagem(
                "Cadastros e listas atualizados.",
                "success"
            );
        } catch (erro) {
            mostrarMensagem(
                `Não foi possível atualizar os dados: ${erro.message}`,
                "error"
            );
        }
    });


document.getElementById("admin-vinculo-profissional")
    .addEventListener("change", carregarServicosAssociados);

document.getElementById("admin-form-associar-servico")
    .addEventListener("submit", async evento => {
        evento.preventDefault();

        const profissionalId = document.getElementById(
            "admin-vinculo-profissional"
        ).value;

        const servicoId = document.getElementById(
            "admin-vinculo-servico"
        ).value;

        const botao = evento.currentTarget.querySelector(
            'button[type="submit"]'
        );

        if (!profissionalId || !servicoId) {
            mostrarFeedbackCadastro(
                "admin-vinculos-feedback",
                "Selecione um profissional e um serviço.",
                "error"
            );
            return;
        }

        botao.disabled = true;

        try {
            await requisicaoApi(
                `/api/profissionais/${encodeURIComponent(profissionalId)}/servicos`,
                {
                    method: "POST",
                    body: JSON.stringify({
                        servicoId: Number(servicoId)
                    })
                }
            );

            mostrarFeedbackCadastro(
                "admin-vinculos-feedback",
                "Serviço associado ao profissional com sucesso.",
                "success"
            );

            await carregarServicosAssociados();
        } catch (erro) {
            mostrarFeedbackCadastro(
                "admin-vinculos-feedback",
                erro.message,
                "error"
            );
        } finally {
            botao.disabled = false;
        }
    });

document.getElementById("admin-disponibilidade-profissional")
    .addEventListener("change", carregarDisponibilidadesAdmin);

document.getElementById("admin-form-disponibilidade")
    .addEventListener("submit", async evento => {
        evento.preventDefault();

        const profissionalId = document.getElementById(
            "admin-disponibilidade-profissional"
        ).value;

        const diaSemana = Number(
            document.getElementById("admin-disponibilidade-dia").value
        );

        const horaInicio = document.getElementById(
            "admin-disponibilidade-inicio"
        ).value;

        const horaFim = document.getElementById(
            "admin-disponibilidade-fim"
        ).value;

        const botao = evento.currentTarget.querySelector(
            'button[type="submit"]'
        );

        if (!profissionalId || !horaInicio || !horaFim) {
            mostrarFeedbackCadastro(
                "admin-disponibilidade-feedback",
                "Preencha o profissional, o horário inicial e o horário final.",
                "error"
            );
            return;
        }

        if (horaInicio >= horaFim) {
            mostrarFeedbackCadastro(
                "admin-disponibilidade-feedback",
                "O horário inicial deve ser anterior ao final.",
                "error"
            );
            return;
        }

        const body = {
            diaSemana,
            horaInicio: `${horaInicio}:00`,
            horaFim: `${horaFim}:00`
        };

        botao.disabled = true;

        try {
            await requisicaoApi(
                `/api/profissionais/${encodeURIComponent(profissionalId)}/disponibilidades`,
                {
                    method: "POST",
                    body: JSON.stringify(body)
                }
            );

            mostrarFeedbackCadastro(
                "admin-disponibilidade-feedback",
                "Disponibilidade cadastrada com sucesso.",
                "success"
            );

            document.getElementById(
                "admin-disponibilidade-inicio"
            ).value = "";

            document.getElementById(
                "admin-disponibilidade-fim"
            ).value = "";

            await carregarDisponibilidadesAdmin();
        } catch (erro) {
            mostrarFeedbackCadastro(
                "admin-disponibilidade-feedback",
                erro.message,
                "error"
            );
        } finally {
            botao.disabled = false;
        }
    });

function preencherSelectGerenciamento(
    select,
    itens,
    textoInicial,
    obterTexto
) {
    const valorAnterior = select.value;

    select.replaceChildren();

    const opcaoInicial = document.createElement("option");
    opcaoInicial.value = "";
    opcaoInicial.textContent = textoInicial;
    select.appendChild(opcaoInicial);

    for (const item of itens) {
        const opcao = document.createElement("option");
        opcao.value = String(item.id);
        opcao.textContent = obterTexto(item);
        select.appendChild(opcao);
    }

    if (itens.some(item => String(item.id) === valorAnterior)) {
        select.value = valorAnterior;
    }
}

async function carregarOpcoesGerenciamento() {
    const [profissionais, servicos] = await Promise.all([
        requisicaoApi("/api/profissionais"),
        requisicaoApi("/api/servicos")
    ]);

    const profissionaisAtivos = profissionais.filter(
        profissional => profissional.ativo
    );

    const profissionaisSelects = [
        document.getElementById("admin-vinculo-profissional"),
        document.getElementById("admin-disponibilidade-profissional")
    ];

    profissionaisSelects.forEach(select => {
        preencherSelectGerenciamento(
            select,
            profissionaisAtivos,
            profissionaisAtivos.length
                ? "Selecione um profissional"
                : "Nenhum profissional ativo",
            profissional =>
                `${profissional.nome} — ${profissional.especialidade}`
        );
    });

    const servicoSelect = document.getElementById(
        "admin-vinculo-servico"
    );

    preencherSelectGerenciamento(
        servicoSelect,
        servicos.filter(servico => servico.ativo),
        "Selecione um serviço",
        servico => `${servico.nome} (${servico.duracaoMinutos} min)`
    );

    await Promise.all([
        carregarServicosAssociados(),
        carregarDisponibilidadesAdmin()
    ]);
}
async function carregarServicosAssociados() {
    const profissionalId = document.getElementById(
        "admin-vinculo-profissional"
    ).value;

    const lista = document.getElementById(
        "admin-servicos-associados"
    );

    lista.replaceChildren();

    if (!profissionalId) {
        const item = document.createElement("li");
        item.className = "management-list-empty";
        item.textContent = "Selecione um profissional.";
        lista.appendChild(item);
        return;
    }

    try {
        const servicos = await requisicaoApi(
            `/api/profissionais/${encodeURIComponent(profissionalId)}/servicos`
        );

        if (servicos.length === 0) {
            const item = document.createElement("li");
            item.className = "management-list-empty";
            item.textContent = "Nenhum serviço associado.";
            lista.appendChild(item);
            return;
        }

        servicos.forEach(servico => {
            const item = document.createElement("li");
            const nome = document.createElement("span");
            const duracao = document.createElement("small");

            nome.textContent = servico.nome;
            duracao.textContent = `${servico.duracaoMinutos} min`;

            item.append(nome, duracao);
            lista.appendChild(item);
        });
    } catch (erro) {
        const item = document.createElement("li");
        item.className = "management-list-empty";
        item.textContent = `Erro ao consultar serviços: ${erro.message}`;
        lista.appendChild(item);
    }
}


async function inicializarPainel() {
    agendaDataInput.value = obterDataLocal();

    try {
        await Promise.all([
            carregarProfissionais(),
            carregarOpcoesGerenciamento()
        ]);
    } catch (erro) {
        mostrarMensagem(
            `Não foi possível carregar os dados: ${erro.message}`,
            "error"
        );
    }

    await Promise.all([
        carregarResumoCadastros(),
        carregarAgenda()
    ]);
}

inicializarPainel();

async function carregarDisponibilidadesAdmin() {
    const profissionalId = document.getElementById(
        "admin-disponibilidade-profissional"
    ).value;

    const lista = document.getElementById(
        "admin-disponibilidades-lista"
    );

    lista.replaceChildren();

    if (!profissionalId) {
        const item = document.createElement("li");
        item.className = "management-list-empty";
        item.textContent = "Selecione um profissional.";
        lista.appendChild(item);
        return;
    }

    try {
        const disponibilidades = await requisicaoApi(
            `/api/profissionais/${encodeURIComponent(profissionalId)}/disponibilidades`
        );

        if (disponibilidades.length === 0) {
            const item = document.createElement("li");
            item.className = "management-list-empty";
            item.textContent = "Nenhuma disponibilidade cadastrada.";
            lista.appendChild(item);
            return;
        }

        disponibilidades.forEach(disponibilidade => {
            const item = document.createElement("li");
            const dia = document.createElement("span");
            const horario = document.createElement("small");

            dia.textContent = disponibilidade.diaSemana;
            horario.textContent =
                `${disponibilidade.horaInicio.slice(0, 5)}–` +
                `${disponibilidade.horaFim.slice(0, 5)}`;

            item.append(dia, horario);
            lista.appendChild(item);
        });
    } catch (erro) {
        const item = document.createElement("li");
        item.className = "management-list-empty";
        item.textContent =
            `Erro ao consultar disponibilidades: ${erro.message}`;

        lista.appendChild(item);
    }
}
inicializarPainel();