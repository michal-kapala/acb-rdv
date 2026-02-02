using System.Collections.Generic;

namespace DDLParserWV
{
    public static class MarkdownRenderer
    {
        public static string RenderProtocol(ProtocolDeclaration protocol, uint version)
        {
            string output = RenderHeader(protocol, version);
            output += "\n";
            output += RenderMethods(protocol, version);
            return output;
        }

        public static string RenderHeader(ProtocolDeclaration protocol, uint version)
        {
            switch(version)
            {
                case 1:
                    return RenderV1Header(protocol);
                case 2:
                    return RenderV2Header(protocol);
                default:
                    return $"[ERROR] Unsupported header rendering for parse tree v{version}";
            }
        }

        public static string RenderV1Header(ProtocolDeclaration protocol)
        {
            string output = $"# {protocol.Declaration.NsItem.TreeItemName}\n";
            output += @"
| Method ID | Method Name |
|-----------|-------------|
";
            uint idx = 1;
            foreach (var item in protocol.NameSpace.Items)
            {
                if (item.Type == EParseTreeElement.RMC)
                {
                    output += RenderV1MethodTableLine((RMC)item, idx);
                    idx++;
                }
            }
            return output;
        }

        public static string RenderV2Header(ProtocolDeclaration protocol)
        {
            string output = $"# {protocol.Declaration.NsItem.TreeItemName}\n";
            output += @"
| ID | Method | Variant |
|----|--------|---------|
";
            uint idx = 1;
            foreach (var item in protocol.NameSpace.Items)
            {
                if (item.Type == EParseTreeElement.RMC)
                    output += RenderV2MethodTableLine((RMC)item, idx);
                idx++;
            }
            return output;
        }

        public static string RenderV1MethodTableLine(RMC method, uint index)
        {
            string name = method.GetName();
            return $"| {index} | [{name}](#{index}-{name.ToLower()}) |\n";
        }

        public static string RenderV2MethodTableLine(RMC method, uint index)
        {
            string name = method.GetName();
            string output = "";
            foreach (var variant in method.MethodDeclaration.V2_Variants1)
            {
                output += $"| {index} | {name} | [{variant.Name}](#{variant.Name.ToLower().Replace(":", "")}) |\n";
                index++;
            }
            return output;
        }

        public static string RenderMethods(ProtocolDeclaration protocol, uint version)
        {
            string output = "";
            uint idx = 1;
            foreach (var item in protocol.NameSpace.Items)
            {
                if (item.Type == EParseTreeElement.RMC)
                {
                    output += RenderMethodDefinition((RMC)item, idx, version);
                    idx++;
                }
            }
            return output;
        }

        public static string RenderMethodDefinition(RMC method, uint index, uint version)
        {
            switch(version)
            {
                case 1:
                    return RenderV1MethodDefinition(method, index);
                case 2:
                    return RenderV2MethodDefinition(method, index);
                default:
                    return $"[ERROR] Unsupported rendering for parse tree v{version}";
            }
        }

        public static string RenderV1MethodDefinition(RMC method, uint index)
        {
            string name = method.GetName();
            string output = $"# ({index}) {name}\n\n";
            output += RenderV1RequestDefinition(method);
            output += "\n";
            output += RenderV1ResponseDefinition(method);
            output += "\n";
            return output;
        }

        public static string RenderV2MethodDefinition(RMC method, uint index)
        {
            string name = method.GetName();
            string output = $"# ({index}) {name}\n\n";
            foreach (var variant in method.MethodDeclaration.V2_Variants1)
                output += RenderV2MethodVariant(variant);
            return output;
        }

        public static string RenderV2MethodVariant(MethodVariant variant)
        {
            string output = $"## {variant.Name}\n\n";
            uint reqParams = 0;
            uint resParams = 0;
            foreach (var item in variant.NameSpace.Items)
            {
                if (item.Type == EParseTreeElement.Parameter)
                {
                    var param = (Parameter)item;
                    if (param.IsRequest())
                        reqParams++;
                    if (param.IsResponse())
                        resParams++;
                }
            }
            output += $"### Request\n";
            if (reqParams == 0)
                output += "\nThis method does not take any parameters.\n";
            else
            {
                output += @"
| Type | Name |
|------|------|
";
                foreach (var elem in variant.NameSpace.Items)
                {
                    if (elem.Type == EParseTreeElement.Parameter)
                    {
                        var param = (Parameter)elem;
                        if (param.IsRequest())
                            output += RenderParameter(param);
                    }
                }
            }
            foreach (var item in variant.NameSpace.Items)
            {
                
            }
            output += $"\n### Response\n";
            if (resParams == 0)
                output += "\nThis method does not return anything.\n";
            else
            {
                output += @"
| Type | Name |
|------|------|
";
                foreach (var elem in variant.NameSpace.Items)
                {
                    if (elem.Type == EParseTreeElement.Parameter)
                    {
                        var param = (Parameter)elem;
                        if (param.IsResponse())
                            output += RenderParameter(param);
                    }
                }
            }
            return output + "\n";
        }

        public static string RenderV1RequestDefinition(RMC method)
        {
            string output = "## Request\n";
            uint reqParams = 0;
            foreach (var item in method.NameSpace.Items)
            {
                if (item.Type == EParseTreeElement.Parameter)
                {
                    var param = (Parameter)item;
                    if (param.IsRequest())
                        reqParams++;
                }
            }

            if (reqParams == 0)
            {
                output += "This method does not take any parameters.\n";
                return output;
            }

            output += @"
| Type | Name |
|------|------|
";
            foreach (var item in method.NameSpace.Items)
            {
                if (item.Type == EParseTreeElement.Parameter)
                {
                    var param = (Parameter)item;
                    if (param.IsRequest())
                        output += RenderParameter(param);
                }
            }
            return output;
        }

        public static string RenderV1ResponseDefinition(RMC method)
        {
            string output = "## Response\n";
            uint resParams = 0;
            foreach (var item in method.NameSpace.Items)
            {
                if (item.Type == EParseTreeElement.ReturnValue)
                    resParams++;
                else if (item.Type == EParseTreeElement.Parameter)
                {
                    var param = (Parameter)item;
                    if (param.IsResponse())
                        resParams++;
                }
            }

            if (resParams == 0)
            {
                output += "This method does not return anything.\n";
                return output;
            }

            output += @"
| Type | Name |
|------|------|
";
            foreach (var item in method.NameSpace.Items)
            {
                if (item.Type == EParseTreeElement.ReturnValue)
                    output += RenderReturnValue((ReturnValue)item);
                else if (item.Type == EParseTreeElement.Parameter)
                {
                    var param = (Parameter)item;
                    if (param.IsResponse())
                        output += RenderParameter(param);
                }
            }
            return output;
        }

        public static string RenderParameter(Parameter param)
        {
            return $"| {param.GetFullType()} | {param.GetName()} |\n";
        }

        public static string RenderReturnValue(ReturnValue retVal)
        {
            return $"| {retVal.GetFullType()} | {retVal.GetName()} |\n";
        }

        public static string RenderClasses(List<ClassDeclaration> classes)
        {
            if (classes.Count == 0)
                return "";

            string output = "# Types\n\n";
            foreach (var type in classes)
                output += RenderClass(type);
            return output;
        }

        public static string RenderClass(ClassDeclaration type)
        {
            uint vars = 0;
            foreach (var item in type.NameSpace.Items)
            {
                switch (item.Type)
                {
                    case EParseTreeElement.Variable:
                        vars++;
                        break;
                    default:
                        break;
                }
            }

            string output = $"## {type.GetName()} ([Structure](https://github.com/kinnay/NintendoClients/wiki/NEX-Common-Types#structure))\n";

            if (type.ParentNamespaceName != "")
            {
                output += $"Extends `{type.ParentNamespaceName}`.\n";
                if (vars == 0)
                    output += "\n";
            }

            // PropertyDeclaration-only class
            if (vars == 0)
            {
                output += "This class does not declare any variables.\n\n";
                return output;
            }
                
            output += @"
| Type | Name |
|------|------|
";
            foreach (var item in type.NameSpace.Items)
            {
                switch (item.Type)
                {
                    case EParseTreeElement.Variable:
                        output += RenderVariable((Variable)item);
                        break;
                    default:
                        break;
                }
            }
            return output + "\n";
        }

        public static string RenderVariable(Variable variable)
        {
            return $"| {variable.GetFullType()} | {variable.GetName()} |\n";
        }
    }
}
