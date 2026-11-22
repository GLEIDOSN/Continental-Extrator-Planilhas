export async function baixarPlanilha(nome, stream) {
    const dados = await stream.arrayBuffer();
    const url = URL.createObjectURL(new Blob([dados], { type: "application/vnd.ms-excel" }));
    try {
        const link = document.createElement("a");
        link.href = url;
        link.download = nome;
        document.body.appendChild(link);
        link.click();
        link.remove();
    } finally {
        URL.revokeObjectURL(url);
    }
}
