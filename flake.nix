{
  description = "Dotnet development environment";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs =
    { nixpkgs, flake-utils, ... }:
    flake-utils.lib.eachDefaultSystem (
      system:
      let
        pkgs = import nixpkgs { inherit system; };

        dotnet = pkgs.dotnetCorePackages.dotnet_10.sdk;
      in
      {
        devShells.default = pkgs.mkShell {
          name = "dotnet-dev";

          packages = with pkgs; [
            dotnet
          ];

          # Environment variables dotnet + the CLR need to behave well
          DOTNET_ROOT = "${dotnet}";
          DOTNET_CLI_TELEMETRY_OPTOUT = "1";
          DOTNET_NOLOGO = "1";

          LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath [
            # pkgs.stdenv.cc.cc
            pkgs.zlib
            pkgs.openssl
            pkgs.icu
          ];

          shellHook = ''
            export TZDIR="${pkgs.tzdata}/share/zoneinfo"
            export NUGET_PACKAGES="''${DIRENV_DIR:-$PWD}/.nuget-packages"
            docker compose up -d
            echo "dotnet dev shell ready"
            echo "  dotnet:  $(dotnet --version)"
          '';
        };
      }
    );
}
