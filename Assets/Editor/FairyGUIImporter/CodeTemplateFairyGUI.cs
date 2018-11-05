using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.CodeDom;
using System.CodeDom.Compiler;
using UnityEditor;


namespace CodeGenerate
{
    public class CodeTemplateFairyGUI
    {

        public static void GenerateCustomElement(string typeName, ExportCustomElementInfo exportCustomElementInfo, string pakID, string pakName)
        {
            //var typeName = typeExportInfo.Key;
            var componentExportInfo = exportCustomElementInfo;

            var thisRef = new CodeThisReferenceExpression();

            CodeNamespace ns = new CodeNamespace(componentExportInfo.ExportPackageName);

            //CodeNamespace uniRxContainer = new CodeNamespace(componentExportInfo.ExportPackageName);

            ns.Imports.Add(new CodeNamespaceImport("System"));
            ns.Imports.Add(new CodeNamespaceImport("System.Collections"));
            ns.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            ns.Imports.Add(new CodeNamespaceImport("UnityEngine"));

            //uniRxContainer.Imports.Add(new CodeNamespaceImport("System"));
            //uniRxContainer.Imports.Add(new CodeNamespaceImport("System.Collections"));
            //uniRxContainer.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            //uniRxContainer.Imports.Add(new CodeNamespaceImport("UnityEngine"));


            //type for handleing ui context
            CodeTypeDeclaration typeDeclaration = new CodeTypeDeclaration(componentExportInfo.ExportTypeName);
            typeDeclaration.IsPartial = true;
            typeDeclaration.IsClass = true;
            
            typeDeclaration.BaseTypes.Add(new CodeTypeReference(componentExportInfo.ExportExtentionType));
            ns.Types.Add(typeDeclaration);

            //support uniRx
            CodeTypeDeclaration typeuniRxWrapper = new CodeTypeDeclaration(componentExportInfo.ExportTypeName + "_rx");
            typeuniRxWrapper.IsClass = true;
            typeuniRxWrapper.BaseTypes.Add(typeof(UIFrame.IRxComponent));
            ns.Types.Add(typeuniRxWrapper);

            //support auto databinding
            CodeTypeDeclaration typeAutoDatabinding = new CodeTypeDeclaration(componentExportInfo.ExportTypeName + "_databind");
            typeAutoDatabinding.IsClass = true;
            //typeAutoDatabinding.BaseTypes.Add()
            ns.Types.Add(typeAutoDatabinding);

            //typeuniRxWrapper config
            {
                var uiBaseField = new CodeMemberField(typeof(UIFrame.UIBase), "_uiBase");
                var uiHandleField = new CodeMemberField(typeName, "_handle");
                typeuniRxWrapper.Members.Add(uiBaseField);
                typeuniRxWrapper.Members.Add(uiHandleField);

                var construct = new CodeConstructor();
                construct.Attributes = MemberAttributes.Public;

                var param1 = new CodeParameterDeclarationExpression(typeof(UIFrame.UIBase), "_uiBase");
                var param2 = new CodeParameterDeclarationExpression(typeName, "_handle");
                construct.Parameters.Add(param1);
                construct.Parameters.Add(param2);
                var statement1 = new CodeAssignStatement(new CodeFieldReferenceExpression(thisRef, "_uiBase"), new CodeArgumentReferenceExpression("_uiBase"));
                var statement2 = new CodeAssignStatement(new CodeFieldReferenceExpression(thisRef, "_handle"), new CodeArgumentReferenceExpression("_handle"));
                construct.Statements.Add(statement1);
                construct.Statements.Add(statement2);
                typeuniRxWrapper.Members.Add(construct);

                //implement IRxComponent 
                var uibaseProp = new CodeMemberProperty()
                {
                    Attributes = MemberAttributes.Public,
                    HasGet = true,
                    Name = "UiBase",
                    Type = new CodeTypeReference(typeof(UIFrame.UIBase))
                };
                var uibaseRefRet = new CodeMethodReturnStatement(new CodeFieldReferenceExpression(thisRef, "_uiBase"));
                uibaseProp.GetStatements.Add(uibaseRefRet);
                typeuniRxWrapper.Members.Add(uibaseProp);

                var objHandleProp = new CodeMemberProperty()
                {
                    Attributes = MemberAttributes.Public,
                    HasGet = true,
                    Name = "GObject",
                    Type = new CodeTypeReference(typeof(FairyGUI.GObject))
                };
                var objHandleRefRet = new CodeMethodReturnStatement(new CodeFieldReferenceExpression(thisRef, "_handle"));
                objHandleProp.GetStatements.Add(objHandleRefRet);
                typeuniRxWrapper.Members.Add(objHandleProp);
            }
            //add url

            CodeMemberField typeUrl = new CodeMemberField(typeof(string), "URL");
            typeUrl.InitExpression = new CodePrimitiveExpression(componentExportInfo.URL);
            typeUrl.Attributes = MemberAttributes.Const;


            //add com id
            {
                var typeofExp = new CodeTypeOfExpression(typeName);
                CodeAttributeDeclaration comIdAtt = new CodeAttributeDeclaration(
                    new CodeTypeReference(typeof(UIFrame.UIAttributes.UIPackageItemIDAttribute)),
                    new CodeAttributeArgument(new CodePrimitiveExpression(string.Concat(componentExportInfo.ID))),
                    new CodeAttributeArgument(typeofExp)
                );

                //add package id
                CodeAttributeDeclaration pakIdAtt = new CodeAttributeDeclaration(
                    new CodeTypeReference(typeof(UIFrame.UIAttributes.UIPackageIDAttribute)),
                    new CodeAttributeArgument(new CodePrimitiveExpression(pakID)),
                    new CodeAttributeArgument(new CodePrimitiveExpression(pakName))
                    );
                typeDeclaration.CustomAttributes.Add(pakIdAtt);
                typeDeclaration.CustomAttributes.Add(comIdAtt);
            }

            typeDeclaration.Members.Add(typeUrl);


            //add static constructor

            {
                var typeConstructor = new CodeTypeConstructor();
                //FairyGUI.UIObjectFactory.SetPackageItemExtension(componentExportInfo.URL, System.Type.GetType(componentExportInfo.ExportTypeName));
                var uiObjectFactoryRef = new CodeTypeReferenceExpression(typeof(FairyGUI.UIObjectFactory));
                var setPackageItemExtensionRef = new CodeMethodReferenceExpression(uiObjectFactoryRef, "SetPackageItemExtension");

                var setPackageItemExtensionInvoke = new CodeMethodInvokeExpression(
                    setPackageItemExtensionRef,
                    new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeName), "URL"),
                    new CodeTypeOfExpression(componentExportInfo.ExportTypeName)
                );

                var gObjectRef = new CodeMemberField(typeof(FairyGUI.GComponent), "_gObject");
                //default constructor
                var defaultConstructor = new CodeConstructor();
                defaultConstructor.Attributes = MemberAttributes.Public;
                typeDeclaration.Members.Add(defaultConstructor);

                //instance constructor
                var instConstructor = new CodeConstructor();
                instConstructor.Attributes = MemberAttributes.Public;
                var param1 = new CodeParameterDeclarationExpression(typeof(FairyGUI.GComponent), "gObject");
                instConstructor.Parameters.Add(param1);
                var statement1 = new CodeAssignStatement(new CodeFieldReferenceExpression(thisRef, "_gObject"), new CodeArgumentReferenceExpression("gObject"));
                instConstructor.Statements.Add(statement1);
                typeConstructor.Statements.Add(setPackageItemExtensionInvoke);
                typeDeclaration.Members.Add(instConstructor);
                typeDeclaration.Members.Add(gObjectRef);
                typeDeclaration.Members.Add(typeConstructor);
            }
            //creator

            {
                var typeCreator = new CodeMemberMethod();
                typeCreator.Attributes = MemberAttributes.Static | MemberAttributes.Public;
                typeCreator.ReturnType = new CodeTypeReference(typeName);
                typeCreator.Name = string.Concat("CreateInstance");
                var typeCreatorStatement1 = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(FairyGUI.UIPackage)), "CreateObject", new CodePrimitiveExpression(pakName), new CodePrimitiveExpression(componentExportInfo.ExportTypeName));
                var typeCreatorStatement2 = new CodeCastExpression(typeName, typeCreatorStatement1);
                var typeCreatorStatement3 = new CodeMethodReturnStatement(typeCreatorStatement2);
                typeCreator.Statements.Add(typeCreatorStatement3);

                var typeCreatorAsyn = new CodeMemberMethod();
                typeCreatorAsyn.Attributes = MemberAttributes.Static | MemberAttributes.Public;
                //typeCreatorAsyn.ReturnType = new CodeTypeReference();
                typeCreatorAsyn.Name = string.Concat("CreateInstanceAsync");
                var typeCreatorAsynParam1 = new CodeParameterDeclarationExpression(typeof(FairyGUI.UIPackage.CreateObjectCallback), "cb");
                typeCreatorAsyn.Parameters.Add(typeCreatorAsynParam1);
                var typeCreatorAsynStatement1 = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(FairyGUI.UIPackage)), "CreateObjectAsync", new CodePrimitiveExpression(pakName), new CodePrimitiveExpression(componentExportInfo.ExportTypeName), new CodeArgumentReferenceExpression("cb"));
                //var typeCreatorAsynStatement2 = new CodeCastExpression(typeName, typeCreatorAsynStatement1);
                //var typeCreatorAsynStatement3 = new CodeMethodReturnStatement(typeCreatorAsynStatement2);
                typeCreatorAsyn.Statements.Add(typeCreatorAsynStatement1);
                typeDeclaration.Members.Add(typeCreatorAsyn);
                typeDeclaration.Members.Add(typeCreator);

            }





            const string memberFieldPrefix = "m_";

            //detect whether or not Override duplicated field name
            componentExportInfo.ExportInfos.ForEach((exportInfo) =>
            {
                var sames = componentExportInfo.ExportInfos.FindAll((obj) => obj.ExportName == exportInfo.ExportName);
                if (sames.Count > 1)
                {
                    for (int idx = 0; idx < sames.Count; idx++)
                    {
                        if (idx > 0)
                            sames[idx].ExportName = string.Concat(sames[idx].ExportName, "_", idx + 1);
                    }
                }
            });



            //add component handle
            componentExportInfo.ExportInfos.ForEach((obj) =>
            {
                string fieldName = string.Concat(memberFieldPrefix, obj.ExportName);
                if (obj.ExportCategory == ExportInfo.EExportCategory.builtInElement || obj.ExportCategory == ExportInfo.EExportCategory.resElement)
                {
                    //fill field and property
                    {
                        CodeMemberField field = new CodeMemberField(obj.ExportType, fieldName);
                        CodeMemberProperty prop = new CodeMemberProperty();
                        prop.Attributes = MemberAttributes.Public;
                        //add id info for dev purpose
                        CodeAttributeDeclaration idAtt = new CodeAttributeDeclaration(
                            new CodeTypeReference(typeof(UIFrame.UIAttributes.UIDisplayItemIDAttribute)),
                            new CodeAttributeArgument(new CodePrimitiveExpression(string.Concat("ID:", obj.ID)))
                        );
                        prop.CustomAttributes.Add(idAtt);
                        prop.Type = new CodeTypeReference(obj.ExportType);
                        prop.Name = string.Concat(obj.ExportName.Substring(0, 1).ToUpper(), obj.ExportName.Substring(1));
                        //new CodeFieldReferenceExpression(thisRef, "_gObject")
                        var getChildExp = new CodeMethodInvokeExpression(thisRef, "GetChildAt", new CodePrimitiveExpression(obj.NodeIndex));
                        var getChildByIdExp = new CodeMethodInvokeExpression(thisRef, "GetChildById", new CodePrimitiveExpression(obj.ID));
                        var var1 = new CodeVariableDeclarationStatement(typeof(FairyGUI.GObject), "tmp");
                        var var2 = new CodeVariableDeclarationStatement(obj.ExportType, "res");
                        var assign1 = new CodeAssignStatement(new CodeVariableReferenceExpression("tmp"), getChildByIdExp);
                        var castExp = new CodeCastExpression(prop.Type, getChildByIdExp);
                        prop.GetStatements.Add(new CodeConditionStatement(
                            new CodeMethodInvokeExpression(
                                new CodeTypeReferenceExpression(typeof(object)), "Equals", new CodeFieldReferenceExpression(thisRef, fieldName),
                                new CodePrimitiveExpression(null)),
                            //assign1,
                            new CodeAssignStatement(new CodeFieldReferenceExpression(thisRef, fieldName), castExp)
                        ));
                        CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(new CodeFieldReferenceExpression(thisRef, fieldName));
                        prop.GetStatements.Add(returnStatement);

                        typeDeclaration.Members.Add(field);
                        typeDeclaration.Members.Add(prop);

                    }
                    //generate rx wrapper field
                    {
                        var assem = System.Reflection.Assembly.GetAssembly(typeof(UIFrame.GObjectSub));
                        var types = assem.GetTypes();
                        var typeRes = System.Array.Find<System.Type>(types, (type) =>
                        {
                            var attrs = type.GetCustomAttributes(typeof(UIFrame.UIAttributes.UIBindTypeInfoAttribute), false);
                            if (attrs.Length > 0)
                            {
                                UIFrame.UIAttributes.UIBindTypeInfoAttribute att = attrs[0] as UIFrame.UIAttributes.UIBindTypeInfoAttribute;
                                return att.BindType == obj.ExportType;
                            }
                            return false;
                        });
                        if (typeRes == null)
                        {
                            if (obj.ExportType.IsSubclassOf(typeof(FairyGUI.GObject)))
                            {
                                typeRes = typeof(UIFrame.GObjectSub);
                            }
                        }
                        if (typeRes != null)
                        {
                            //var attrs = typeRes.GetCustomAttributes(typeof(UIFrame.UIAttributes.UIBindTypeInfoAttribute), false);
                            //UIFrame.UIAttributes.UIBindTypeInfoAttribute att = attrs[0] as UIFrame.UIAttributes.UIBindTypeInfoAttribute;
                            //if (att.BindType == obj.ExportType)
                            {
                                //var rxField = new CodeMemberField(typeRes, fieldName + "_rx");
                                //typeuniRxWrapper.Members.Add(rxField);
                                var varDef = new CodeVariableDeclarationStatement(typeRes, obj.ExportName + "_rx");
                                var varRef = new CodeVariableReferenceExpression(obj.ExportName + "_rx");
                                var fieldRef = new CodeFieldReferenceExpression(thisRef, fieldName + "_rx");

                                var rxProp = new CodeMemberProperty();
                                rxProp.Attributes = MemberAttributes.Public;
                                rxProp.Type = new CodeTypeReference(typeRes);
                                rxProp.Name = string.Concat(obj.ExportName.Substring(0, 1).ToUpper(), obj.ExportName.Substring(1)); ;// + "_rx"
                                var getEle = new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(thisRef, "_handle"), rxProp.Name);
                                //var isEqualExp = new CodeMethodInvokeExpression(fieldRef, "Equals", new CodePrimitiveExpression(null));
                                var createRxWpInst = new CodeObjectCreateExpression(typeRes,
                                                                                    new CodeFieldReferenceExpression(thisRef, "_uiBase"),
                                                                                    getEle
                                                                                   );

                                var assignExp1 = new CodeAssignStatement(varRef, createRxWpInst);
                                //var condition1 = new CodeConditionStatement(isEqualExp,assignExp1);
                                var retExp = new CodeMethodReturnStatement(varRef);
                                rxProp.GetStatements.Add(varDef);
                                rxProp.GetStatements.Add(assignExp1);
                                rxProp.GetStatements.Add(retExp);
                                typeuniRxWrapper.Members.Add(rxProp);
                            }
                        }

                    }

                }
                else if (obj.ExportCategory == ExportInfo.EExportCategory.customComponent)
                {
                    //fill field and property
                    {
                        CodeMemberField field = new CodeMemberField(obj.ExportTypeName, fieldName);
                        CodeMemberProperty prop = new CodeMemberProperty();
                        prop.Attributes = MemberAttributes.Public;
                        //add id info for dev purpose
                        CodeAttributeDeclaration idAtt = new CodeAttributeDeclaration(
                            new CodeTypeReference(typeof(UIFrame.UIAttributes.UIDisplayItemIDAttribute)),
                            new CodeAttributeArgument(new CodePrimitiveExpression(string.Concat("ID:", obj.ID)))
                        );
                        prop.CustomAttributes.Add(idAtt);
                        prop.Type = new CodeTypeReference(obj.ExportTypeName, CodeTypeReferenceOptions.GenericTypeParameter);
                        prop.Name = string.Concat(obj.ExportName.Substring(0, 1).ToUpper(), obj.ExportName.Substring(1));
                        //new CodeFieldReferenceExpression(thisRef, "_gObject")
                        var getChildExp = new CodeMethodInvokeExpression(thisRef, "GetChildAt", new CodePrimitiveExpression(obj.NodeIndex));
                        var getChildByIdExp = new CodeMethodInvokeExpression(thisRef, "GetChildById", new CodePrimitiveExpression(obj.ID));
                        var var1 = new CodeVariableDeclarationStatement(typeof(FairyGUI.GObject), "tmp");
                        var var2 = new CodeVariableDeclarationStatement(obj.ExportTypeName, "res");
                        var assign1 = new CodeAssignStatement(new CodeVariableReferenceExpression("tmp"), getChildByIdExp);
                        var castExp = new CodeCastExpression(prop.Type, getChildByIdExp);
                        prop.GetStatements.Add(new CodeConditionStatement(
                            new CodeMethodInvokeExpression(
                                new CodeTypeReferenceExpression(typeof(object)), "Equals", new CodeFieldReferenceExpression(thisRef, fieldName),
                                new CodePrimitiveExpression(null)),
                            //assign1,
                            new CodeAssignStatement(new CodeFieldReferenceExpression(thisRef, fieldName), castExp)
                        ));
                        CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(new CodeFieldReferenceExpression(thisRef, fieldName));
                        prop.GetStatements.Add(returnStatement);
                        typeDeclaration.Members.Add(field);
                        typeDeclaration.Members.Add(prop);
                    }


                    //generate rx wrapper field
                    {
                        var assem = System.Reflection.Assembly.GetAssembly(typeof(UIFrame.GObjectSub));
                        var types = assem.GetTypes();
                        var typeRes = System.Array.Find<System.Type>(types, (type) =>
                        {
                            var attrs = type.GetCustomAttributes(typeof(UIFrame.UIAttributes.UIBindTypeInfoAttribute), false);
                            if (attrs.Length > 0)
                            {
                                UIFrame.UIAttributes.UIBindTypeInfoAttribute att = attrs[0] as UIFrame.UIAttributes.UIBindTypeInfoAttribute;
                                return att.BindType == typeof(FairyGUI.GComponent);
                            }
                            return false;
                        });
                        if (typeRes == null)
                        {
                            if (obj.ExportType.IsSubclassOf(typeof(FairyGUI.GObject)))
                            {
                                typeRes = typeof(UIFrame.GObjectSub);
                            }
                        }
                        if (typeRes != null)
                        {
                            //var attrs = typeRes.GetCustomAttributes(typeof(UIFrame.UIAttributes.UIBindTypeInfoAttribute), false);
                            //UIFrame.UIAttributes.UIBindTypeInfoAttribute att = attrs[0] as UIFrame.UIAttributes.UIBindTypeInfoAttribute;
                            //if (att.BindType == obj.ExportType)
                            {
                                //var rxField = new CodeMemberField(typeRes, fieldName + "_rx");
                                //typeuniRxWrapper.Members.Add(rxField);
                                var varDef = new CodeVariableDeclarationStatement(typeRes, obj.ExportName + "_rx");
                                var varRef = new CodeVariableReferenceExpression(obj.ExportName + "_rx");
                                var fieldRef = new CodeFieldReferenceExpression(thisRef, fieldName + "_rx");

                                var rxProp = new CodeMemberProperty();
                                rxProp.Attributes = MemberAttributes.Public;
                                rxProp.Type = new CodeTypeReference(typeRes);
                                rxProp.Name = string.Concat(obj.ExportName.Substring(0, 1).ToUpper(), obj.ExportName.Substring(1));// + "_rx"
                                var getEle = new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(thisRef, "_handle"), rxProp.Name);
                                //var isEqualExp = new CodeMethodInvokeExpression(fieldRef, "Equals", new CodePrimitiveExpression(null));
                                var createRxWpInst = new CodeObjectCreateExpression(typeRes,
                                                                                    new CodeFieldReferenceExpression(thisRef, "_uiBase"),
                                                                                    getEle
                                                                                   );

                                var assignExp1 = new CodeAssignStatement(varRef, createRxWpInst);
                                //var condition1 = new CodeConditionStatement(isEqualExp,assignExp1);
                                var retExp = new CodeMethodReturnStatement(varRef);
                                rxProp.GetStatements.Add(varDef);
                                rxProp.GetStatements.Add(assignExp1);
                                rxProp.GetStatements.Add(retExp);
                                typeuniRxWrapper.Members.Add(rxProp);
                            }
                            //generate component_rx
                            {
                                var dyType = new CodeTypeReference(obj.ExportTypeName + "_rx");
                                //var varDef = new CodeVariableDeclarationStatement(dyType, obj.ExportName + "_com_rx");
                                //var varRef = new CodeVariableReferenceExpression(obj.ExportName + "_com_rx");
                                //var fieldRef = new CodeFieldReferenceExpression(thisRef, fieldName + "_rx");
                                //只对包内自定义控件导出Rx代理
                                if (obj.isTypeInPak)
                                {
                                    var rxProp = new CodeMemberProperty();
                                    rxProp.Attributes = MemberAttributes.Public;
                                    rxProp.Type = dyType;
                                    rxProp.Name = string.Concat(obj.ExportName.Substring(0, 1).ToUpper(), obj.ExportName.Substring(1), "_typed");// + "_rx"
                                    var getUIBase = new CodeFieldReferenceExpression(thisRef, "_uiBase");
                                    var getHandle = new CodeFieldReferenceExpression(thisRef, "_handle");
                                    var getEle = new CodePropertyReferenceExpression(getHandle, string.Concat(obj.ExportName.Substring(0, 1).ToUpper(), obj.ExportName.Substring(1)));
                                    var constructRx = new CodeObjectCreateExpression(dyType, getUIBase, getEle);
                                    var ret = new CodeMethodReturnStatement(constructRx);
                                    rxProp.GetStatements.Add(ret);
                                    typeuniRxWrapper.Members.Add(rxProp);
                                }
                            }
                        }

                    }

                }
                else if (obj.ExportCategory == ExportInfo.EExportCategory.customElement)
                {
                    //fill field and property
                    {
                        CodeMemberField field = new CodeMemberField(obj.ExportTypeName, fieldName);
                        CodeMemberProperty prop = new CodeMemberProperty();
                        prop.Attributes = MemberAttributes.Public;
                        //add id info for dev purpose
                        CodeAttributeDeclaration idAtt = new CodeAttributeDeclaration(
                            new CodeTypeReference(typeof(UIFrame.UIAttributes.UIDisplayItemIDAttribute)),
                            new CodeAttributeArgument(new CodePrimitiveExpression(string.Concat("ID:", obj.ID)))
                        );
                        prop.CustomAttributes.Add(idAtt);
                        prop.Type = new CodeTypeReference(obj.ExportTypeName, CodeTypeReferenceOptions.GenericTypeParameter);
                        prop.Name = string.Concat(obj.ExportName.Substring(0, 1).ToUpper(), obj.ExportName.Substring(1));
                        //new CodeFieldReferenceExpression(thisRef, "_gObject")
                        var getChildExp = new CodeMethodInvokeExpression(thisRef, "GetChildAt", new CodePrimitiveExpression(obj.NodeIndex));
                        var getChildByIdExp = new CodeMethodInvokeExpression(thisRef, "GetChildById", new CodePrimitiveExpression(obj.ID));
                        var var1 = new CodeVariableDeclarationStatement(typeof(FairyGUI.GObject), "tmp");
                        var var2 = new CodeVariableDeclarationStatement(obj.ExportTypeName, "res");
                        var assign1 = new CodeAssignStatement(new CodeVariableReferenceExpression("tmp"), getChildByIdExp);
                        var castExp = new CodeCastExpression(prop.Type, getChildByIdExp);
                        prop.GetStatements.Add(new CodeConditionStatement(
                            new CodeMethodInvokeExpression(
                                //new CodeFieldReferenceExpression(thisRef, fieldName), "Equals", new CodePrimitiveExpression(null)),
                                new CodeTypeReferenceExpression(typeof(object)), "Equals", new CodeFieldReferenceExpression(thisRef, fieldName),
                                new CodePrimitiveExpression(null)),
                            new CodeAssignStatement(new CodeFieldReferenceExpression(thisRef, fieldName), castExp)
                        ));
                        CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(new CodeFieldReferenceExpression(thisRef, fieldName));
                        prop.GetStatements.Add(returnStatement);
                        typeDeclaration.Members.Add(field);
                        typeDeclaration.Members.Add(prop);


                    }

                    //generate rx wrapper field
                    {
                        var assem = System.Reflection.Assembly.GetAssembly(typeof(UIFrame.GObjectSub));
                        var types = assem.GetTypes();
                        var typeRes = System.Array.Find<System.Type>(types, (type) =>
                        {
                            var attrs = type.GetCustomAttributes(typeof(UIFrame.UIAttributes.UIBindTypeInfoAttribute), false);
                            if (attrs.Length > 0)
                            {
                                UIFrame.UIAttributes.UIBindTypeInfoAttribute att = attrs[0] as UIFrame.UIAttributes.UIBindTypeInfoAttribute;
                                return att.BindType == obj.ExtentionType;
                            }
                            return false;
                        });
                        if (typeRes == null)
                        {

                            if (obj.ExtentionType.IsSubclassOf(typeof(FairyGUI.GObject)))
                            {
                                typeRes = typeof(UIFrame.GObjectSub);
                            }
                        }
                        if (typeRes != null)
                        {
                            //var attrs = typeRes.GetCustomAttributes(typeof(UIFrame.UIAttributes.UIBindTypeInfoAttribute), false);
                            //UIFrame.UIAttributes.UIBindTypeInfoAttribute att = attrs[0] as UIFrame.UIAttributes.UIBindTypeInfoAttribute;
                            //if (att.BindType == obj.ExportType)
                            {
                                //var rxField = new CodeMemberField(typeRes, fieldName + "_rx");
                                //typeuniRxWrapper.Members.Add(rxField);
                                var varDef = new CodeVariableDeclarationStatement(typeRes, obj.ExportName + "_rx");
                                var varRef = new CodeVariableReferenceExpression(obj.ExportName + "_rx");
                                var fieldRef = new CodeFieldReferenceExpression(thisRef, fieldName + "_rx");

                                var rxProp = new CodeMemberProperty();
                                rxProp.Attributes = MemberAttributes.Public;
                                rxProp.Type = new CodeTypeReference(typeRes);
                                rxProp.Name = string.Concat(obj.ExportName.Substring(0, 1).ToUpper(), obj.ExportName.Substring(1)); // + "_rx"
                                var getEle = new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(thisRef, "_handle"), rxProp.Name);
                                //var isEqualExp = new CodeMethodInvokeExpression(fieldRef, "Equals", new CodePrimitiveExpression(null));
                                var createRxWpInst = new CodeObjectCreateExpression(typeRes,
                                                                                    new CodeFieldReferenceExpression(thisRef, "_uiBase"),
                                                                                    getEle
                                                                                   );

                                var assignExp1 = new CodeAssignStatement(varRef, createRxWpInst);
                                //var condition1 = new CodeConditionStatement(isEqualExp,assignExp1);
                                var retExp = new CodeMethodReturnStatement(varRef);
                                rxProp.GetStatements.Add(varDef);
                                rxProp.GetStatements.Add(assignExp1);
                                rxProp.GetStatements.Add(retExp);
                                typeuniRxWrapper.Members.Add(rxProp);
                            }
                        }

                    }
                }
            });


            CodeFieldReferenceExpression gameObjRef = new CodeFieldReferenceExpression(thisRef, "_gameObject");

            if (!System.IO.Directory.Exists(CodeGeneratorSetting.ExportPath + pakName))
            {
                System.IO.Directory.CreateDirectory(CodeGeneratorSetting.ExportPath + pakName);
            }
            OutPutCode(CodeGeneratorSetting.ExportPath + pakName + "/" + typeName + ".cs", ns);
        }

        public static void GenerateCustomComponent(string typeName, ExportComponentInfo exportComponentInfo, string pakID, string pakName)
        {
            //var typeName = typeExportInfo.Key;
            var componentExportInfo = exportComponentInfo;

            var thisRef = new CodeThisReferenceExpression();

            CodeNamespace ns = new CodeNamespace(componentExportInfo.ExportPackageName);

            //CodeNamespace uniRxContainer = new CodeNamespace(componentExportInfo.ExportPackageName);

            ns.Imports.Add(new CodeNamespaceImport("System"));
            ns.Imports.Add(new CodeNamespaceImport("System.Collections"));
            ns.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            ns.Imports.Add(new CodeNamespaceImport("UnityEngine"));

            //uniRxContainer.Imports.Add(new CodeNamespaceImport("System"));
            //uniRxContainer.Imports.Add(new CodeNamespaceImport("System.Collections"));
            //uniRxContainer.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            //uniRxContainer.Imports.Add(new CodeNamespaceImport("UnityEngine"));


            //type for handleing ui context
            CodeTypeDeclaration typeDeclaration = new CodeTypeDeclaration(componentExportInfo.ExportTypeName);
            typeDeclaration.IsPartial = true;
            typeDeclaration.IsClass = true;
            typeDeclaration.BaseTypes.Add(new CodeTypeReference(typeof(FairyGUI.GComponent)));
            ns.Types.Add(typeDeclaration);

            //support uniRx
            CodeTypeDeclaration typeuniRxWrapper = new CodeTypeDeclaration(componentExportInfo.ExportTypeName + "_rx");
            typeuniRxWrapper.IsClass = true;
            typeuniRxWrapper.BaseTypes.Add(typeof(UIFrame.IRxComponent));
            ns.Types.Add(typeuniRxWrapper);

            //support auto databinding
            CodeTypeDeclaration typeAutoDatabinding = new CodeTypeDeclaration(componentExportInfo.ExportTypeName + "_databind");
            typeAutoDatabinding.IsClass = true;
            //typeAutoDatabinding.BaseTypes.Add()
            ns.Types.Add(typeAutoDatabinding);

            //typeuniRxWrapper config
            {
                var uiBaseField = new CodeMemberField(typeof(UIFrame.UIBase), "_uiBase");
                var uiHandleField = new CodeMemberField(typeName, "_handle");
                typeuniRxWrapper.Members.Add(uiBaseField);
                typeuniRxWrapper.Members.Add(uiHandleField);

                var construct = new CodeConstructor();
                construct.Attributes = MemberAttributes.Public;

                var param1 = new CodeParameterDeclarationExpression(typeof(UIFrame.UIBase), "_uiBase");
                var param2 = new CodeParameterDeclarationExpression(typeName, "_handle");
                construct.Parameters.Add(param1);
                construct.Parameters.Add(param2);
                var statement1 = new CodeAssignStatement(new CodeFieldReferenceExpression(thisRef, "_uiBase"), new CodeArgumentReferenceExpression("_uiBase"));
                var statement2 = new CodeAssignStatement(new CodeFieldReferenceExpression(thisRef, "_handle"), new CodeArgumentReferenceExpression("_handle"));
                construct.Statements.Add(statement1);
                construct.Statements.Add(statement2);
                typeuniRxWrapper.Members.Add(construct);

                //implement IRxComponent 
                var uibaseProp = new CodeMemberProperty()
                {
                    Attributes = MemberAttributes.Public,
                    HasGet = true,
                    Name = "UiBase",
                    Type = new CodeTypeReference(typeof(UIFrame.UIBase))
                };
                var uibaseRefRet = new CodeMethodReturnStatement(new CodeFieldReferenceExpression(thisRef, "_uiBase"));
                uibaseProp.GetStatements.Add(uibaseRefRet);
                typeuniRxWrapper.Members.Add(uibaseProp);

                var objHandleProp = new CodeMemberProperty()
                {
                    Attributes = MemberAttributes.Public,
                    HasGet = true,
                    Name = "GObject",
                    Type = new CodeTypeReference(typeof(FairyGUI.GObject))
                };
                var objHandleRefRet = new CodeMethodReturnStatement(new CodeFieldReferenceExpression(thisRef, "_handle"));
                objHandleProp.GetStatements.Add(objHandleRefRet);
                typeuniRxWrapper.Members.Add(objHandleProp);
            }
            //add url

            CodeMemberField typeUrl = new CodeMemberField(typeof(string), "URL");
            typeUrl.InitExpression = new CodePrimitiveExpression(componentExportInfo.URL);
            typeUrl.Attributes = MemberAttributes.Const;


            //add com id
            {
                var typeofExp = new CodeTypeOfExpression(typeName);
                CodeAttributeDeclaration comIdAtt = new CodeAttributeDeclaration(
                    new CodeTypeReference(typeof(UIFrame.UIAttributes.UIPackageItemIDAttribute)),
                    new CodeAttributeArgument(new CodePrimitiveExpression(string.Concat(componentExportInfo.ID))),
                    new CodeAttributeArgument(typeofExp)
                );

                //add package id
                CodeAttributeDeclaration pakIdAtt = new CodeAttributeDeclaration(
                    new CodeTypeReference(typeof(UIFrame.UIAttributes.UIPackageIDAttribute)),
                    new CodeAttributeArgument(new CodePrimitiveExpression(pakID)),
                    new CodeAttributeArgument(new CodePrimitiveExpression(pakName))
                    );
                typeDeclaration.CustomAttributes.Add(pakIdAtt);
                typeDeclaration.CustomAttributes.Add(comIdAtt);
            }

            typeDeclaration.Members.Add(typeUrl);


            //add static constructor

            {
                var typeConstructor = new CodeTypeConstructor();
                //FairyGUI.UIObjectFactory.SetPackageItemExtension(componentExportInfo.URL, System.Type.GetType(componentExportInfo.ExportTypeName));
                var uiObjectFactoryRef = new CodeTypeReferenceExpression(typeof(FairyGUI.UIObjectFactory));
                var setPackageItemExtensionRef = new CodeMethodReferenceExpression(uiObjectFactoryRef, "SetPackageItemExtension");

                var setPackageItemExtensionInvoke = new CodeMethodInvokeExpression(
                    setPackageItemExtensionRef,
                    new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeName), "URL"),
                    new CodeTypeOfExpression(componentExportInfo.ExportTypeName)
                );

                var gObjectRef = new CodeMemberField(typeof(FairyGUI.GComponent), "_gObject");
                //default constructor
                var defaultConstructor = new CodeConstructor();
                defaultConstructor.Attributes = MemberAttributes.Public;
                typeDeclaration.Members.Add(defaultConstructor);

                //instance constructor
                var instConstructor = new CodeConstructor();
                instConstructor.Attributes = MemberAttributes.Public;
                var param1 = new CodeParameterDeclarationExpression(typeof(FairyGUI.GComponent), "gObject");
                instConstructor.Parameters.Add(param1);
                var statement1 = new CodeAssignStatement(new CodeFieldReferenceExpression(thisRef, "_gObject"), new CodeArgumentReferenceExpression("gObject"));
                instConstructor.Statements.Add(statement1);
                typeConstructor.Statements.Add(setPackageItemExtensionInvoke);
                typeDeclaration.Members.Add(instConstructor);
                typeDeclaration.Members.Add(gObjectRef);
                typeDeclaration.Members.Add(typeConstructor);
            }
            //creator

            {
                var typeCreator = new CodeMemberMethod();
                typeCreator.Attributes = MemberAttributes.Static | MemberAttributes.Public;
                typeCreator.ReturnType = new CodeTypeReference(typeName);
                typeCreator.Name = string.Concat("CreateInstance");
                var typeCreatorStatement1 = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(FairyGUI.UIPackage)), "CreateObject", new CodePrimitiveExpression(pakName), new CodePrimitiveExpression(componentExportInfo.ExportTypeName));
                var typeCreatorStatement2 = new CodeCastExpression(typeName, typeCreatorStatement1);
                var typeCreatorStatement3 = new CodeMethodReturnStatement(typeCreatorStatement2);
                typeCreator.Statements.Add(typeCreatorStatement3);

                var typeCreatorAsyn = new CodeMemberMethod();
                typeCreatorAsyn.Attributes = MemberAttributes.Static | MemberAttributes.Public;
                //typeCreatorAsyn.ReturnType = new CodeTypeReference();
                typeCreatorAsyn.Name = string.Concat("CreateInstanceAsync");
                var typeCreatorAsynParam1 = new CodeParameterDeclarationExpression(typeof(FairyGUI.UIPackage.CreateObjectCallback), "cb");
                typeCreatorAsyn.Parameters.Add(typeCreatorAsynParam1);
                var typeCreatorAsynStatement1 = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(FairyGUI.UIPackage)), "CreateObjectAsync", new CodePrimitiveExpression(pakName), new CodePrimitiveExpression(componentExportInfo.ExportTypeName), new CodeArgumentReferenceExpression("cb"));
                //var typeCreatorAsynStatement2 = new CodeCastExpression(typeName, typeCreatorAsynStatement1);
                //var typeCreatorAsynStatement3 = new CodeMethodReturnStatement(typeCreatorAsynStatement2);
                typeCreatorAsyn.Statements.Add(typeCreatorAsynStatement1);
                typeDeclaration.Members.Add(typeCreatorAsyn);
                typeDeclaration.Members.Add(typeCreator);

            }





            const string memberFieldPrefix = "m_";

            //detect whether or not Override duplicated field name
            componentExportInfo.ExportInfos.ForEach((exportInfo) =>
            {
                var sames = componentExportInfo.ExportInfos.FindAll((obj) => obj.ExportName == exportInfo.ExportName);
                if (sames.Count > 1)
                {
                    for (int idx = 0; idx < sames.Count; idx++)
                    {
                        if (idx > 0)
                            sames[idx].ExportName = string.Concat(sames[idx].ExportName, "_", idx + 1);
                    }
                }
            });



            //add component handle
            componentExportInfo.ExportInfos.ForEach((obj) =>
            {
                string fieldName = string.Concat(memberFieldPrefix, obj.ExportName);
                if (obj.ExportCategory == ExportInfo.EExportCategory.builtInElement || obj.ExportCategory == ExportInfo.EExportCategory.resElement)
                {
                    //fill field and property
                    {
                        CodeMemberField field = new CodeMemberField(obj.ExportType, fieldName);
                        CodeMemberProperty prop = new CodeMemberProperty();
                        prop.Attributes = MemberAttributes.Public;
                        //add id info for dev purpose
                        CodeAttributeDeclaration idAtt = new CodeAttributeDeclaration(
                            new CodeTypeReference(typeof(UIFrame.UIAttributes.UIDisplayItemIDAttribute)),
                            new CodeAttributeArgument(new CodePrimitiveExpression(string.Concat("ID:", obj.ID)))
                        );
                        prop.CustomAttributes.Add(idAtt);
                        prop.Type = new CodeTypeReference(obj.ExportType);
                        prop.Name = string.Concat(obj.ExportName.Substring(0, 1).ToUpper(), obj.ExportName.Substring(1));
                        //new CodeFieldReferenceExpression(thisRef, "_gObject")
                        var getChildExp = new CodeMethodInvokeExpression(thisRef, "GetChildAt", new CodePrimitiveExpression(obj.NodeIndex));
                        var getChildByIdExp = new CodeMethodInvokeExpression(thisRef, "GetChildById", new CodePrimitiveExpression(obj.ID));
                        var var1 = new CodeVariableDeclarationStatement(typeof(FairyGUI.GObject), "tmp");
                        var var2 = new CodeVariableDeclarationStatement(obj.ExportType, "res");
                        var assign1 = new CodeAssignStatement(new CodeVariableReferenceExpression("tmp"), getChildByIdExp);
                        var castExp = new CodeCastExpression(prop.Type, getChildByIdExp);
                        prop.GetStatements.Add(new CodeConditionStatement(
                            new CodeMethodInvokeExpression(
                                new CodeTypeReferenceExpression(typeof(object)), "Equals", new CodeFieldReferenceExpression(thisRef, fieldName),
                                new CodePrimitiveExpression(null)),
                            //assign1,
                            new CodeAssignStatement(new CodeFieldReferenceExpression(thisRef, fieldName), castExp)
                        ));
                        CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(new CodeFieldReferenceExpression(thisRef, fieldName));
                        prop.GetStatements.Add(returnStatement);

                        typeDeclaration.Members.Add(field);
                        typeDeclaration.Members.Add(prop);

                    }
                    //generate rx wrapper field
                    {
                        var assem = System.Reflection.Assembly.GetAssembly(typeof(UIFrame.GObjectSub));
                        var types = assem.GetTypes();
                        var typeRes = System.Array.Find<System.Type>(types, (type) =>
                        {
                            var attrs = type.GetCustomAttributes(typeof(UIFrame.UIAttributes.UIBindTypeInfoAttribute), false);
                            if (attrs.Length > 0)
                            {
                                UIFrame.UIAttributes.UIBindTypeInfoAttribute att = attrs[0] as UIFrame.UIAttributes.UIBindTypeInfoAttribute;
                                return att.BindType == obj.ExportType;
                            }
                            return false;
                        });
                        if (typeRes == null)
                        {
                            if (obj.ExportType.IsSubclassOf(typeof(FairyGUI.GObject)))
                            {
                                typeRes = typeof(UIFrame.GObjectSub);
                            }
                        }
                        if (typeRes != null)
                        {
                            //var attrs = typeRes.GetCustomAttributes(typeof(UIFrame.UIAttributes.UIBindTypeInfoAttribute), false);
                            //UIFrame.UIAttributes.UIBindTypeInfoAttribute att = attrs[0] as UIFrame.UIAttributes.UIBindTypeInfoAttribute;
                            //if (att.BindType == obj.ExportType)
                            {
                                //var rxField = new CodeMemberField(typeRes, fieldName + "_rx");
                                //typeuniRxWrapper.Members.Add(rxField);
                                var varDef = new CodeVariableDeclarationStatement(typeRes, obj.ExportName + "_rx");
                                var varRef = new CodeVariableReferenceExpression(obj.ExportName + "_rx");
                                var fieldRef = new CodeFieldReferenceExpression(thisRef, fieldName + "_rx");

                                var rxProp = new CodeMemberProperty();
                                rxProp.Attributes = MemberAttributes.Public;
                                rxProp.Type = new CodeTypeReference(typeRes);
                                rxProp.Name = string.Concat(obj.ExportName.Substring(0, 1).ToUpper(), obj.ExportName.Substring(1)); ;// + "_rx"
                                var getEle = new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(thisRef, "_handle"), rxProp.Name);
                                //var isEqualExp = new CodeMethodInvokeExpression(fieldRef, "Equals", new CodePrimitiveExpression(null));
                                var createRxWpInst = new CodeObjectCreateExpression(typeRes,
                                                                                    new CodeFieldReferenceExpression(thisRef, "_uiBase"),
                                                                                    getEle
                                                                                   );

                                var assignExp1 = new CodeAssignStatement(varRef, createRxWpInst);
                                //var condition1 = new CodeConditionStatement(isEqualExp,assignExp1);
                                var retExp = new CodeMethodReturnStatement(varRef);
                                rxProp.GetStatements.Add(varDef);
                                rxProp.GetStatements.Add(assignExp1);
                                rxProp.GetStatements.Add(retExp);
                                typeuniRxWrapper.Members.Add(rxProp);
                            }
                        }

                    }

                }
                else if (obj.ExportCategory == ExportInfo.EExportCategory.customComponent)
                {
                    //fill field and property
                    {
                        CodeMemberField field = new CodeMemberField(obj.ExportTypeName, fieldName);
                        CodeMemberProperty prop = new CodeMemberProperty();
                        prop.Attributes = MemberAttributes.Public;
                        //add id info for dev purpose
                        CodeAttributeDeclaration idAtt = new CodeAttributeDeclaration(
                            new CodeTypeReference(typeof(UIFrame.UIAttributes.UIDisplayItemIDAttribute)),
                            new CodeAttributeArgument(new CodePrimitiveExpression(string.Concat("ID:", obj.ID)))
                        );
                        prop.CustomAttributes.Add(idAtt);
                        prop.Type = new CodeTypeReference(obj.ExportTypeName, CodeTypeReferenceOptions.GenericTypeParameter);
                        prop.Name = string.Concat(obj.ExportName.Substring(0, 1).ToUpper(), obj.ExportName.Substring(1));
                        //new CodeFieldReferenceExpression(thisRef, "_gObject")
                        var getChildExp = new CodeMethodInvokeExpression(thisRef, "GetChildAt", new CodePrimitiveExpression(obj.NodeIndex));
                        var getChildByIdExp = new CodeMethodInvokeExpression(thisRef, "GetChildById", new CodePrimitiveExpression(obj.ID));
                        var var1 = new CodeVariableDeclarationStatement(typeof(FairyGUI.GObject), "tmp");
                        var var2 = new CodeVariableDeclarationStatement(obj.ExportTypeName, "res");
                        var assign1 = new CodeAssignStatement(new CodeVariableReferenceExpression("tmp"), getChildByIdExp);
                        var castExp = new CodeCastExpression(prop.Type, getChildByIdExp);
                        prop.GetStatements.Add(new CodeConditionStatement(
                            new CodeMethodInvokeExpression(
                                new CodeTypeReferenceExpression(typeof(object)), "Equals", new CodeFieldReferenceExpression(thisRef, fieldName),
                                new CodePrimitiveExpression(null)),
                            //assign1,
                            new CodeAssignStatement(new CodeFieldReferenceExpression(thisRef, fieldName), castExp)
                        ));
                        CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(new CodeFieldReferenceExpression(thisRef, fieldName));
                        prop.GetStatements.Add(returnStatement);
                        typeDeclaration.Members.Add(field);
                        typeDeclaration.Members.Add(prop);
                    }


                    //generate rx wrapper field
                    {
                        var assem = System.Reflection.Assembly.GetAssembly(typeof(UIFrame.GObjectSub));
                        var types = assem.GetTypes();
                        var typeRes = System.Array.Find<System.Type>(types, (type) =>
                        {
                            var attrs = type.GetCustomAttributes(typeof(UIFrame.UIAttributes.UIBindTypeInfoAttribute), false);
                            if (attrs.Length > 0)
                            {
                                UIFrame.UIAttributes.UIBindTypeInfoAttribute att = attrs[0] as UIFrame.UIAttributes.UIBindTypeInfoAttribute;
                                return att.BindType == typeof(FairyGUI.GComponent);
                            }
                            return false;
                        });
                        if (typeRes == null)
                        {
                            if (obj.ExportType.IsSubclassOf(typeof(FairyGUI.GObject)))
                            {
                                typeRes = typeof(UIFrame.GObjectSub);
                            }
                        }
                        if (typeRes != null)
                        {
                            //var attrs = typeRes.GetCustomAttributes(typeof(UIFrame.UIAttributes.UIBindTypeInfoAttribute), false);
                            //UIFrame.UIAttributes.UIBindTypeInfoAttribute att = attrs[0] as UIFrame.UIAttributes.UIBindTypeInfoAttribute;
                            //if (att.BindType == obj.ExportType)
                            {
                                //var rxField = new CodeMemberField(typeRes, fieldName + "_rx");
                                //typeuniRxWrapper.Members.Add(rxField);
                                var varDef = new CodeVariableDeclarationStatement(typeRes, obj.ExportName + "_rx");
                                var varRef = new CodeVariableReferenceExpression(obj.ExportName + "_rx");
                                var fieldRef = new CodeFieldReferenceExpression(thisRef, fieldName + "_rx");

                                var rxProp = new CodeMemberProperty();
                                rxProp.Attributes = MemberAttributes.Public;
                                rxProp.Type = new CodeTypeReference(typeRes);
                                rxProp.Name = string.Concat(obj.ExportName.Substring(0, 1).ToUpper(), obj.ExportName.Substring(1));// + "_rx"
                                var getEle = new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(thisRef, "_handle"), rxProp.Name);
                                //var isEqualExp = new CodeMethodInvokeExpression(fieldRef, "Equals", new CodePrimitiveExpression(null));
                                var createRxWpInst = new CodeObjectCreateExpression(typeRes,
                                                                                    new CodeFieldReferenceExpression(thisRef, "_uiBase"),
                                                                                    getEle
                                                                                   );

                                var assignExp1 = new CodeAssignStatement(varRef, createRxWpInst);
                                //var condition1 = new CodeConditionStatement(isEqualExp,assignExp1);
                                var retExp = new CodeMethodReturnStatement(varRef);
                                rxProp.GetStatements.Add(varDef);
                                rxProp.GetStatements.Add(assignExp1);
                                rxProp.GetStatements.Add(retExp);
                                typeuniRxWrapper.Members.Add(rxProp);
                            }
                            //generate component_rx
                            {
                                var dyType = new CodeTypeReference(obj.ExportTypeName + "_rx");
                                //var varDef = new CodeVariableDeclarationStatement(dyType, obj.ExportName + "_com_rx");
                                //var varRef = new CodeVariableReferenceExpression(obj.ExportName + "_com_rx");
                                //var fieldRef = new CodeFieldReferenceExpression(thisRef, fieldName + "_rx");
                                //只对包内组件导出子节点Rx代理
                                if (obj.isTypeInPak)
                                {
                                    var rxProp = new CodeMemberProperty();
                                    rxProp.Attributes = MemberAttributes.Public;
                                    rxProp.Type = dyType;
                                    rxProp.Name = string.Concat(obj.ExportName.Substring(0, 1).ToUpper(), obj.ExportName.Substring(1), "_typed");// + "_rx"
                                    var getUIBase = new CodeFieldReferenceExpression(thisRef, "_uiBase");
                                    var getHandle = new CodeFieldReferenceExpression(thisRef, "_handle");
                                    var getEle = new CodePropertyReferenceExpression(getHandle, string.Concat(obj.ExportName.Substring(0, 1).ToUpper(), obj.ExportName.Substring(1)));
                                    var constructRx = new CodeObjectCreateExpression(dyType, getUIBase, getEle);
                                    var ret = new CodeMethodReturnStatement(constructRx);
                                    rxProp.GetStatements.Add(ret);
                                    typeuniRxWrapper.Members.Add(rxProp);
                                }
                            }
                        }

                    }

                }
                else if (obj.ExportCategory == ExportInfo.EExportCategory.customElement)
                {
                    //fill field and property
                    {
                        CodeMemberField field = new CodeMemberField(obj.ExportTypeName, fieldName);
                        CodeMemberProperty prop = new CodeMemberProperty();
                        prop.Attributes = MemberAttributes.Public;
                        //add id info for dev purpose
                        CodeAttributeDeclaration idAtt = new CodeAttributeDeclaration(
                            new CodeTypeReference(typeof(UIFrame.UIAttributes.UIDisplayItemIDAttribute)),
                            new CodeAttributeArgument(new CodePrimitiveExpression(string.Concat("ID:", obj.ID)))
                        );
                        prop.CustomAttributes.Add(idAtt);
                        prop.Type = new CodeTypeReference(obj.ExportTypeName, CodeTypeReferenceOptions.GenericTypeParameter);
                        prop.Name = string.Concat(obj.ExportName.Substring(0, 1).ToUpper(), obj.ExportName.Substring(1));
                        //new CodeFieldReferenceExpression(thisRef, "_gObject")
                        var getChildExp = new CodeMethodInvokeExpression(thisRef, "GetChildAt", new CodePrimitiveExpression(obj.NodeIndex));
                        var getChildByIdExp = new CodeMethodInvokeExpression(thisRef, "GetChildById", new CodePrimitiveExpression(obj.ID));
                        var var1 = new CodeVariableDeclarationStatement(typeof(FairyGUI.GObject), "tmp");
                        var var2 = new CodeVariableDeclarationStatement(obj.ExportTypeName, "res");
                        var assign1 = new CodeAssignStatement(new CodeVariableReferenceExpression("tmp"), getChildByIdExp);
                        var castExp = new CodeCastExpression(prop.Type, getChildByIdExp);
                        prop.GetStatements.Add(new CodeConditionStatement(
                            new CodeMethodInvokeExpression(
                                //new CodeFieldReferenceExpression(thisRef, fieldName), "Equals", new CodePrimitiveExpression(null)),
                                new CodeTypeReferenceExpression(typeof(object)), "Equals", new CodeFieldReferenceExpression(thisRef, fieldName),
                                new CodePrimitiveExpression(null)),
                            new CodeAssignStatement(new CodeFieldReferenceExpression(thisRef, fieldName), castExp)
                        ));
                        CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(new CodeFieldReferenceExpression(thisRef, fieldName));
                        prop.GetStatements.Add(returnStatement);
                        typeDeclaration.Members.Add(field);
                        typeDeclaration.Members.Add(prop);


                    }

                    //generate rx wrapper field
                    {
                        var assem = System.Reflection.Assembly.GetAssembly(typeof(UIFrame.GObjectSub));
                        var types = assem.GetTypes();
                        var typeRes = System.Array.Find<System.Type>(types, (type) =>
                        {
                            var attrs = type.GetCustomAttributes(typeof(UIFrame.UIAttributes.UIBindTypeInfoAttribute), false);
                            if (attrs.Length > 0)
                            {
                                UIFrame.UIAttributes.UIBindTypeInfoAttribute att = attrs[0] as UIFrame.UIAttributes.UIBindTypeInfoAttribute;
                                return att.BindType == obj.ExtentionType;
                            }
                            return false;
                        });
                        if (typeRes == null)
                        {

                            if (obj.ExtentionType.IsSubclassOf(typeof(FairyGUI.GObject)))
                            {
                                typeRes = typeof(UIFrame.GObjectSub);
                            }
                        }
                        if (typeRes != null)
                        {
                            //var attrs = typeRes.GetCustomAttributes(typeof(UIFrame.UIAttributes.UIBindTypeInfoAttribute), false);
                            //UIFrame.UIAttributes.UIBindTypeInfoAttribute att = attrs[0] as UIFrame.UIAttributes.UIBindTypeInfoAttribute;
                            //if (att.BindType == obj.ExportType)
                            {
                                //var rxField = new CodeMemberField(typeRes, fieldName + "_rx");
                                //typeuniRxWrapper.Members.Add(rxField);
                                var varDef = new CodeVariableDeclarationStatement(typeRes, obj.ExportName + "_rx");
                                var varRef = new CodeVariableReferenceExpression(obj.ExportName + "_rx");
                                var fieldRef = new CodeFieldReferenceExpression(thisRef, fieldName + "_rx");

                                var rxProp = new CodeMemberProperty();
                                rxProp.Attributes = MemberAttributes.Public;
                                rxProp.Type = new CodeTypeReference(typeRes);
                                rxProp.Name = string.Concat(obj.ExportName.Substring(0, 1).ToUpper(), obj.ExportName.Substring(1)); // + "_rx"
                                var getEle = new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(thisRef, "_handle"), rxProp.Name);
                                //var isEqualExp = new CodeMethodInvokeExpression(fieldRef, "Equals", new CodePrimitiveExpression(null));
                                var createRxWpInst = new CodeObjectCreateExpression(typeRes,
                                                                                    new CodeFieldReferenceExpression(thisRef, "_uiBase"),
                                                                                    getEle
                                                                                   );

                                var assignExp1 = new CodeAssignStatement(varRef, createRxWpInst);
                                //var condition1 = new CodeConditionStatement(isEqualExp,assignExp1);
                                var retExp = new CodeMethodReturnStatement(varRef);
                                rxProp.GetStatements.Add(varDef);
                                rxProp.GetStatements.Add(assignExp1);
                                rxProp.GetStatements.Add(retExp);
                                typeuniRxWrapper.Members.Add(rxProp);
                            }
                        }

                    }
                }
            });


            CodeFieldReferenceExpression gameObjRef = new CodeFieldReferenceExpression(thisRef, "_gameObject");

            if (!System.IO.Directory.Exists(CodeGeneratorSetting.ExportPath + pakName))
            {
                System.IO.Directory.CreateDirectory(CodeGeneratorSetting.ExportPath + pakName);
            }
            OutPutCode(CodeGeneratorSetting.ExportPath + pakName + "/" + typeName + ".cs", ns);
        }

        [UnityEditor.MenuItem("Tools/OutPutCode")]
        public static void OutPutCode(string path, CodeNamespace ns)
        {
            CodeNamespace nameSpace = ns;


            System.CodeDom.Compiler.CodeDomProvider provider = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("CSharp");
            bool supportStatic = provider.Supports(GeneratorSupport.PublicStaticMembers);
            bool supportStaticConstruct = provider.Supports(GeneratorSupport.StaticConstructors);

            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            CodeCompileUnit targetUnit = new CodeCompileUnit();
            targetUnit.Namespaces.Add(nameSpace);

            using (System.IO.StreamWriter sourceWriter = new System.IO.StreamWriter(path))
            {
                provider.GenerateCodeFromCompileUnit(
                    targetUnit, sourceWriter, options);
            }

            AssetDatabase.ImportAsset(FileUtil.GetProjectRelativePath(path), ImportAssetOptions.ForceUpdate);
        }
    }


}
