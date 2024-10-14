FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /froxy

COPY source/bin/net8.0/ .

ENV FORWARD=""
ENV LOCAL=""
ENV REMOTE=""
ENV USER=""
ENV PASSWORD=""

CMD ["sh", "-c", "dotnet froxy.dll --forward $FORWARD --local $LOCAL --remote $REMOTE --user $USER --pass $PASSWORD"]