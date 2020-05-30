using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HuNN
{
    public static class ExportHelper
    {
        public static void ExportNetwork(Network network)
        {
            var dn = GetHelperNetwork(network);

            var dialog = new SaveFileDialog
            {
                Title = "Save Network File",
                Filter = "Text File|*.txt;"
            };
        
            if (dialog.ShowDialog() != true) return;
            using (var file = File.CreateText(dialog.FileName))
            {
                var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                serializer.Serialize(file, dn);
            }          
        }

        public static void ExportDatasets(List<DataSet> datasets)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Save Dataset File",
                Filter = "Text File|*.txt;"
            };


            if (dialog.ShowDialog() != true) return;
            using (var file = File.CreateText(dialog.FileName))
            {
                var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                serializer.Serialize(file, datasets);
            }
        }

        public static void ExportDatasetsAsCSV(List<DataSet> datasets)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Save Dataset File",
                Filter = "Text File|*.txt;"
            };
            if (dialog.ShowDialog() != true) return;

            ProgressDialogResult result = ProgressDialog.ProgressDialog.Execute(Application.Current.MainWindow, "Converting Dataset...", () => {
                using (var file = File.CreateText(dialog.FileName))
                {
                    //var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                    //serializer.Serialize(file, datasets);
                    foreach(DataSet ds in datasets)
                    {
                        string s = "";
                        foreach(double d in ds.Targets)
                        {
                            s += d.ToString() + ",";
                        }
                        foreach (double d in ds.Values)
                        {
                            s += d.ToString() + ",";
                        } 
                        file.WriteLine(s.Substring(0,s.Length-1));
                    }
               
                }
            });
        }





        private static HelperNetwork GetHelperNetwork(Network network)
        {
            var hn = new HelperNetwork
            {
                LearnRate = network.LearnRate,
                Momentum = network.Momentum
            };

            //Input Layer
            foreach (var n in network.InputLayer)
            {
                var neuron = new HelperNeuron
                {
                    Id = n.Id,
                    Bias = n.Bias,
                    BiasDelta = n.BiasDelta,
                    Gradient = n.Gradient,
                    InputValue = n.InputValue,
                    OutputValue = n.OutputValue,
                    Error = n.Error
                };

                hn.InputLayer.Add(neuron);

                foreach (var synapse in n.OutputSynapses)
                {
                    var syn = new HelperSynapse
                    {
                        Id = synapse.Id,
                        OutputNeuronId = synapse.OutputNeuron.Id,
                        InputNeuronId = synapse.InputNeuron.Id,
                        Weight = synapse.Weight,
                        WeightDelta = synapse.WeightDelta
                    };

                    hn.Synapses.Add(syn);
                }
            }

            //Hidden Layer
            foreach (var l in network.HiddenLayers)
            {
                var layer = new List<HelperNeuron>();

                foreach (var n in l)
                {
                    var neuron = new HelperNeuron
                    {
                        Id = n.Id,
                        Bias = n.Bias,
                        BiasDelta = n.BiasDelta,
                        Gradient = n.Gradient,
                        InputValue = n.InputValue,
                        OutputValue = n.OutputValue,
                        Error = n.Error
                    };

                    layer.Add(neuron);

                    foreach (var synapse in n.OutputSynapses)
                    {
                        var syn = new HelperSynapse
                        {
                            Id = synapse.Id,
                            OutputNeuronId = synapse.OutputNeuron.Id,
                            InputNeuronId = synapse.InputNeuron.Id,
                            Weight = synapse.Weight,
                            WeightDelta = synapse.WeightDelta
                        };

                        hn.Synapses.Add(syn);
                    }
                }

                hn.HiddenLayers.Add(layer);
            }

            //Output Layer
            foreach (var n in network.OutputLayer)
            {
                var neuron = new HelperNeuron
                {
                    Id = n.Id,
                    Bias = n.Bias,
                    BiasDelta = n.BiasDelta,
                    Gradient = n.Gradient,
                    InputValue = n.InputValue,
                    OutputValue=n.OutputValue,
                    Error=n.Error
                };

                hn.OutputLayer.Add(neuron);

                foreach (var synapse in n.OutputSynapses)
                {
                    var syn = new HelperSynapse
                    {
                        Id = synapse.Id,
                        OutputNeuronId = synapse.OutputNeuron.Id,
                        InputNeuronId = synapse.InputNeuron.Id,
                        Weight = synapse.Weight,
                        WeightDelta = synapse.WeightDelta
                    };

                    hn.Synapses.Add(syn);
                }
            }
            return hn;
        }



    }
}
