# Deploy a Virtal Machine in Azure with IaC

In this lab, you will deploy a virtual machine in Azure using Infrastructure as Code (IaC). You will use an Azure Resource Manager (ARM) template to deploy the virtual machine.

## Create an ARM template

````json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "adminUsername": {
      "type": "string",
      "metadata": {
        "description": "Username for the Virtual Machine."
      }
    },
    "adminPassword": {
      "type": "securestring",
      "minLength": 12,
      "metadata": {
        "description": "Password for the Virtual Machine."
      }
    },
    "dnsLabelPrefix": {
      "type": "string",
      "defaultValue": "[toLower(format('{0}-{1}', parameters('vmName'), uniqueString(resourceGroup().id, parameters('vmName'))))]",
      "metadata": {
        "description": "Unique DNS Name for the Public IP used to access the Virtual Machine."
      }
    },
    "publicIpName": {
      "type": "string",
      "defaultValue": "myPublicIP",
      "metadata": {
        "description": "Name for the Public IP used to access the Virtual Machine."
      }
    },
    "publicIPAllocationMethod": {
      "type": "string",
      "defaultValue": "Dynamic",
      "allowedValues": [
        "Dynamic",
        "Static"
      ],
      "metadata": {
        "description": "Allocation method for the Public IP used to access the Virtual Machine."
      }
    },
    "publicIpSku": {
      "type": "string",
      "defaultValue": "Basic",
      "allowedValues": [
        "Basic",
        "Standard"
      ],
      "metadata": {
        "description": "SKU for the Public IP used to access the Virtual Machine."
      }
    },
    "OSVersion": {
      "type": "string",
      "defaultValue": "2022-datacenter-azure-edition",
      "allowedValues": [
        "2016-datacenter-gensecond",
        "2016-datacenter-server-core-g2",
        "2016-datacenter-server-core-smalldisk-g2",
        "2016-datacenter-smalldisk-g2",
        "2016-datacenter-with-containers-g2",
        "2016-datacenter-zhcn-g2",
        "2019-datacenter-core-g2",
        "2019-datacenter-core-smalldisk-g2",
        "2019-datacenter-core-with-containers-g2",
        "2019-datacenter-core-with-containers-smalldisk-g2",
        "2019-datacenter-gensecond",
        "2019-datacenter-smalldisk-g2",
        "2019-datacenter-with-containers-g2",
        "2019-datacenter-with-containers-smalldisk-g2",
        "2019-datacenter-zhcn-g2",
        "2022-datacenter-azure-edition",
        "2022-datacenter-azure-edition-core",
        "2022-datacenter-azure-edition-core-smalldisk",
        "2022-datacenter-azure-edition-smalldisk",
        "2022-datacenter-core-g2",
        "2022-datacenter-core-smalldisk-g2",
        "2022-datacenter-g2",
        "2022-datacenter-smalldisk-g2"
      ],
      "metadata": {
        "description": "The Windows version for the VM. This will pick a fully patched image of this given Windows version."
      }
    },
    "vmSize": {
      "type": "string",
      "defaultValue": "Standard_D2s_v5",
      "metadata": {
        "description": "Size of the virtual machine."
      }
    },
    "location": {
      "type": "string",
      "defaultValue": "[resourceGroup().location]",
      "metadata": {
        "description": "Location for all resources."
      }
    },
    "vmName": {
      "type": "string",
      "defaultValue": "simple-vm",
      "metadata": {
        "description": "Name of the virtual machine."
      }
    },
    "securityType": {
      "type": "string",
      "defaultValue": "TrustedLaunch",
      "allowedValues": [
        "Standard",
        "TrustedLaunch"
      ],
      "metadata": {
        "description": "Security Type of the Virtual Machine."
      }
    }
  },
  "variables": {
    "storageAccountName": "[format('bootdiags{0}', uniqueString(resourceGroup().id))]",
    "nicName": "myVMNic",
    "addressPrefix": "10.0.0.0/16",
    "subnetName": "Subnet",
    "subnetPrefix": "10.0.0.0/24",
    "virtualNetworkName": "MyVNET",
    "networkSecurityGroupName": "default-NSG",
    "securityProfileJson": {
      "uefiSettings": {
        "secureBootEnabled": true,
        "vTpmEnabled": true
      },
      "securityType": "[parameters('securityType')]"
    },
    "extensionName": "GuestAttestation",
    "extensionPublisher": "Microsoft.Azure.Security.WindowsAttestation",
    "extensionVersion": "1.0",
    "maaTenantName": "GuestAttestation",
    "maaEndpoint": "[substring('emptyString', 0, 0)]"
  },
  "resources": [
    {
      "type": "Microsoft.Storage/storageAccounts",
      "apiVersion": "2022-05-01",
      "name": "[variables('storageAccountName')]",
      "location": "[parameters('location')]",
      "sku": {
        "name": "Standard_LRS"
      },
      "kind": "Storage"
    },
    {
      "type": "Microsoft.Network/publicIPAddresses",
      "apiVersion": "2022-05-01",
      "name": "[parameters('publicIpName')]",
      "location": "[parameters('location')]",
      "sku": {
        "name": "[parameters('publicIpSku')]"
      },
      "properties": {
        "publicIPAllocationMethod": "[parameters('publicIPAllocationMethod')]",
        "dnsSettings": {
          "domainNameLabel": "[parameters('dnsLabelPrefix')]"
        }
      }
    },
    {
      "type": "Microsoft.Network/networkSecurityGroups",
      "apiVersion": "2022-05-01",
      "name": "[variables('networkSecurityGroupName')]",
      "location": "[parameters('location')]",
      "properties": {
        "securityRules": [
          {
            "name": "default-allow-3389",
            "properties": {
              "priority": 1000,
              "access": "Allow",
              "direction": "Inbound",
              "destinationPortRange": "3389",
              "protocol": "Tcp",
              "sourcePortRange": "*",
              "sourceAddressPrefix": "*",
              "destinationAddressPrefix": "*"
            }
          }
        ]
      }
    },
    {
      "type": "Microsoft.Network/virtualNetworks",
      "apiVersion": "2022-05-01",
      "name": "[variables('virtualNetworkName')]",
      "location": "[parameters('location')]",
      "properties": {
        "addressSpace": {
          "addressPrefixes": [
            "[variables('addressPrefix')]"
          ]
        },
        "subnets": [
          {
            "name": "[variables('subnetName')]",
            "properties": {
              "addressPrefix": "[variables('subnetPrefix')]",
              "networkSecurityGroup": {
                "id": "[resourceId('Microsoft.Network/networkSecurityGroups', variables('networkSecurityGroupName'))]"
              }
            }
          }
        ]
      },
      "dependsOn": [
        "[resourceId('Microsoft.Network/networkSecurityGroups', variables('networkSecurityGroupName'))]"
      ]
    },
    {
      "type": "Microsoft.Network/networkInterfaces",
      "apiVersion": "2022-05-01",
      "name": "[variables('nicName')]",
      "location": "[parameters('location')]",
      "properties": {
        "ipConfigurations": [
          {
            "name": "ipconfig1",
            "properties": {
              "privateIPAllocationMethod": "Dynamic",
              "publicIPAddress": {
                "id": "[resourceId('Microsoft.Network/publicIPAddresses', parameters('publicIpName'))]"
              },
              "subnet": {
                "id": "[resourceId('Microsoft.Network/virtualNetworks/subnets', variables('virtualNetworkName'), variables('subnetName'))]"
              }
            }
          }
        ]
      },
      "dependsOn": [
        "[resourceId('Microsoft.Network/publicIPAddresses', parameters('publicIpName'))]",
        "[resourceId('Microsoft.Network/virtualNetworks', variables('virtualNetworkName'))]"
      ]
    },
    {
      "type": "Microsoft.Compute/virtualMachines",
      "apiVersion": "2022-03-01",
      "name": "[parameters('vmName')]",
      "location": "[parameters('location')]",
      "properties": {
        "hardwareProfile": {
          "vmSize": "[parameters('vmSize')]"
        },
        "osProfile": {
          "computerName": "[parameters('vmName')]",
          "adminUsername": "[parameters('adminUsername')]",
          "adminPassword": "[parameters('adminPassword')]"
        },
        "storageProfile": {
          "imageReference": {
            "publisher": "MicrosoftWindowsServer",
            "offer": "WindowsServer",
            "sku": "[parameters('OSVersion')]",
            "version": "latest"
          },
          "osDisk": {
            "createOption": "FromImage",
            "managedDisk": {
              "storageAccountType": "StandardSSD_LRS"
            }
          },
          "dataDisks": [
            {
              "diskSizeGB": 1023,
              "lun": 0,
              "createOption": "Empty"
            }
          ]
        },
        "networkProfile": {
          "networkInterfaces": [
            {
              "id": "[resourceId('Microsoft.Network/networkInterfaces', variables('nicName'))]"
            }
          ]
        },
        "diagnosticsProfile": {
          "bootDiagnostics": {
            "enabled": true,
            "storageUri": "[reference(resourceId('Microsoft.Storage/storageAccounts', variables('storageAccountName')), '2022-05-01').primaryEndpoints.blob]"
          }
        },
        "securityProfile": "[if(equals(parameters('securityType'), 'TrustedLaunch'), variables('securityProfileJson'), null())]"
      },
      "dependsOn": [
        "[resourceId('Microsoft.Network/networkInterfaces', variables('nicName'))]",
        "[resourceId('Microsoft.Storage/storageAccounts', variables('storageAccountName'))]"
      ]
    },
    {
      "condition": "[and(equals(parameters('securityType'), 'TrustedLaunch'), and(equals(variables('securityProfileJson').uefiSettings.secureBootEnabled, true()), equals(variables('securityProfileJson').uefiSettings.vTpmEnabled, true())))]",
      "type": "Microsoft.Compute/virtualMachines/extensions",
      "apiVersion": "2022-03-01",
      "name": "[format('{0}/{1}', parameters('vmName'), variables('extensionName'))]",
      "location": "[parameters('location')]",
      "properties": {
        "publisher": "[variables('extensionPublisher')]",
        "type": "[variables('extensionName')]",
        "typeHandlerVersion": "[variables('extensionVersion')]",
        "autoUpgradeMinorVersion": true,
        "enableAutomaticUpgrade": true,
        "settings": {
          "AttestationConfig": {
            "MaaSettings": {
              "maaEndpoint": "[variables('maaEndpoint')]",
              "maaTenantName": "[variables('maaTenantName')]"
            }
          }
        }
      },
      "dependsOn": [
        "[resourceId('Microsoft.Compute/virtualMachines', parameters('vmName'))]"
      ]
    }
  ],
  "outputs": {
    "hostname": {
      "type": "string",
      "value": "[reference(resourceId('Microsoft.Network/publicIPAddresses', parameters('publicIpName')), '2022-05-01').dnsSettings.fqdn]"
    }
  }
}
````

## Deploy the ARM template

### Prérequis

- Un compte Azure DevOps.
- Un projet Azure DevOps.
- Un repository contenant votre code source et les fichiers ARM template.

### Configuration du Pipeline

1. Créez un fichier `azure-pipelines.yml` à la racine de votre repository avec le contenu suivant :

    ```yaml
    trigger:
    - main

    pool:
      vmImage: 'ubuntu-latest'

    variables:
      armTemplatePath: 'path/to/your/armTemplate'
      armTemplateParametersPath: 'path/to/your/armTemplateParameters'

    steps:
    - task: CopyFiles@2
      inputs:
        SourceFolder: '$(System.DefaultWorkingDirectory)/$(armTemplatePath)'
        Contents: '**'
        TargetFolder: '$(Build.ArtifactStagingDirectory)/arm-template'

    - task: CopyFiles@2
      inputs:
        SourceFolder: '$(System.DefaultWorkingDirectory)/$(armTemplateParametersPath)'
        Contents: '**'
        TargetFolder: '$(Build.ArtifactStagingDirectory)/arm-template'

    - task: PublishBuildArtifacts@1
      inputs:
        PathtoPublish: '$(Build.ArtifactStagingDirectory)/arm-template'
        ArtifactName: 'arm-template'
        publishLocation: 'Container'
    ```

2. Remplacez `path/to/your/armTemplate` et `path/to/your/armTemplateParameters` par les chemins réels de vos fichiers ARM template et des paramètres dans votre repository.

3. Commitez et poussez ce fichier dans votre repository.

### Déclenchement du Pipeline

Le pipeline se déclenchera automatiquement à chaque fois qu'il y a un commit sur la branche `main`. Vous pouvez changer la branche de déclenchement en modifiant la section `trigger` du fichier YAML.

### Accéder aux Artifacts

Une fois le pipeline exécuté, vous pourrez accéder aux artefacts générés dans la section **Artifacts** de votre build dans Azure DevOps.

## Créer une Pipeline de Release
### 2. Créer une Nouvelle Pipeline de Release

- Cliquez sur **Pipelines** dans le menu de gauche.
- Sélectionnez **Releases**.
- Cliquez sur **New pipeline**.

### 3. Configurer la Source des Artefacts

- Cliquez sur **Add an artifact**.
- Dans la section **Source type**, sélectionnez **Build**.
- Choisissez votre projet.
- Dans la section **Source (build pipeline)**, sélectionnez la pipeline de build que vous avez configurée précédemment.
- Dans la section **Default version**, sélectionnez la version du build à utiliser (par exemple, Latest).
- Cliquez sur **Add**.

### 4. Définir l'Environnement de Déploiement

- Cliquez sur **Stage 1** pour configurer un nouvel environnement de déploiement. Renommez ce stage en fonction de votre environnement (par exemple, "Dev", "Test", "Production").
- Cliquez sur **Add** sous **Tasks** pour ajouter des tâches à ce stage.

### 5. Ajouter une Tâche de Déploiement ARM

- Dans le volet **Add tasks**, recherchez **Azure Resource Group Deployment** et ajoutez cette tâche.
- Configurez les paramètres de la tâche :
  - **Azure subscription** : Sélectionnez votre abonnement Azure (vous devrez peut-être autoriser Azure DevOps à accéder à votre abonnement).
  - **Action** : Sélectionnez **Create or update resource group**.
  - **Resource group** : Choisissez ou spécifiez le groupe de ressources à utiliser.
  - **Location** : Sélectionnez la région où déployer les ressources.
  - **Template location** : Sélectionnez **Linked artifact** et choisissez le fichier ARM template que vous avez publié comme artefact.
  - **Template parameters** : Sélectionnez le fichier de paramètres ARM si vous en utilisez un.
  - **Override template parameters** : Ajoutez des paramètres supplémentaires ou remplacez ceux existants si nécessaire.

#### Exemple de Configuration des Chemins des Templates

- **Template**: `$(System.DefaultWorkingDirectory)/drop/arm-template/armTemplate.json`
- **Template parameters**: `$(System.DefaultWorkingDirectory)/drop/arm-template/armTemplateParameters.json`

### 6. Configurer les Variables et Conditions

- Dans le panneau de configuration du stage, vous pouvez définir des variables spécifiques à l'environnement.
- Configurez les conditions de déclenchement (par exemple, manuel, après le succès d'un autre stage, etc.).

### 7. Enregistrer et Créer la Release

- Cliquez sur **Save** pour enregistrer la configuration de la pipeline.
- Donnez un nom à votre pipeline de release si nécessaire.
- Cliquez sur **Create release** pour déclencher la première release.
- Suivez les instructions pour déployer les artefacts.

## Accéder aux Artifacts

Une fois le pipeline exécuté, vous pourrez accéder aux artefacts générés dans la section **Artifacts** de votre build dans Azure DevOps.
