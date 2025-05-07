const quill = new Quill('#editor', {
    modules: {
        toolbar: [
            [{ header: [1, 2, false] }],
            ['bold', 'italic', 'underline'],
            ['code-block'],
        ],
    },
    placeholder: 'Coloque aqui la entrada...',
    theme: 'snow', // or 'bubble'
});

function btnEnviarClick() {
    let isValido = validarFormularioCompleto();

    if (!isValido) {
        return;
    }

    const delta = quill.getContents();
    const deltaJSON = JSON.stringify(delta.ops);
    $("#Cuerpo").val(deltaJSON);
    $("#formEntrada").trigger("submit");
}

function validarFormularioCompleto() {
    let formEntradaEsValido = $("#formEntrada").valid();
    let cuerpoEsValido = validarCuerpo();

    return formEntradaEsValido && cuerpoEsValido;
}

function validarCuerpo() {
    let mensajeError = null;
    let isValido = true;

    const htmlBody = quill.getSemanticHTML();

    if (htmlBody === '<p></p>') {
        mensajeError = "El cuerpo es requerido";
        isValido = false;
    }

    return isValido;
}

quill.on('text-change', function (delta, oldDelta, source) {
    validarCuerpo();
});