{
  description = "Arthea .NET 10 local/CI toolchain (not a deployable server)";

  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";

  outputs = { self, nixpkgs }:
    let
      system = "x86_64-linux";
      pkgs = import nixpkgs { inherit system; };
    in
    {
      devShells.${system}.ci = pkgs.mkShell {
        packages = with pkgs; [
          dotnet-sdk_10
          python3
          cacert
        ];

        DOTNET_CLI_TELEMETRY_OPTOUT = "1";
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1";
        DOTNET_NOLOGO = "1";
      };

      devShells.${system}.default = self.devShells.${system}.ci;
    };
}
