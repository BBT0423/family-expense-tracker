export function download(contentType: any, fileContents: string, fileName: string): void {
  const byteCharacters = atob(fileContents);
  const byteNumbers = new Array(byteCharacters.length);
  for (let i = 0; i < byteCharacters.length; i++) {
    byteNumbers[i] = byteCharacters.charCodeAt(i);
  }
  const byteArray = new Uint8Array(byteNumbers);
  const blob = new Blob([byteArray], { type: contentType });
  const url = globalThis.URL.createObjectURL(blob);

  if (contentType === 'application/pdf') {
    // For PDF files, open in a new tab for preview
    const newWindow = globalThis.open(url, '_blank');
    if (newWindow) {
      newWindow.onload = () => {
        const timer = setTimeout(() => {
          newWindow.document.title = fileName;
        }, 1000);

        newWindow.onbeforeunload = () => {
          clearTimeout(timer);
        };
      };
    }
  } else {
    // For other file types, proceed with download
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', fileName);
    document.body.appendChild(link);
    link.click();
    link.remove();
  }
}
