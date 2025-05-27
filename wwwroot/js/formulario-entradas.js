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

function cargarContenido(contenido) {
    quill.setContents(contenido, 'silent');
}

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

function mostrarPrevisualizar(event) {
    const input = event.target;
    const imagenPreview = document.getElementById("PreviewImagen");

    if (input.files && input.files[0]) {
        const urlImagen = URL.createObjectURL(input.files[0]);
        imagenPreview.src = urlImagen;
        imagenPreview.style.display = "block";
    }
}

async function generarImagen() {
    const titulo = document.getElementById("Titulo").value;

    if (!titulo) {
        alert('El titulo no puede estar vacio');
        return;
    }

    const imagenPortadaInput = document.getElementById("ImagenPortada");
    imagenPortadaInput.value = '';

    const imagenPreview = document.getElementById("PreviewImagen");
    imagenPreview.style.display = "none";

    const loading = document.getElementById("loading-imagen-ia");
    loading.style.display = "block";

    const respuesta = await fetch("/Entradas/GenerarImagen?Titulo=" + encodeURIComponent(titulo));

    if (!respuesta.ok) {
        const contenido = await respuesta.text();
        alert(contenido);
        return;
    }

    const blob = await respuesta.blob();
    imagenPreview.src = URL.createObjectURL(blob);
    imagenPreview.style.display = "block";
    loading.style.display = "none";

    const base64string = await convertirBlobABase64(blob);
    document.getElementById("ImagenPortadaIA").value = base64string;
}

async function convertirBlobABase64(blob) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(blob);
        reader.onloadend = () => {
            const base64string = reader.result.split(",")[1];
            resolve(base64string);
        }

        reader.onerror = reject;
    });
}