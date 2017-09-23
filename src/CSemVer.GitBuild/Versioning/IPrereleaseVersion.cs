namespace CSemVer.GitBuild
{
    public interface IPrereleaseVersion
    {
        (int NameIndex, byte Number, byte Fix) Version { get; }

        string ToString( bool useFullPrerelNames );
    }
}
