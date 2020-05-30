using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace HuNN
{
    public static class ImportHelper
    {

        public static Network ImportNetwork()
        {
            var dn = GetHelperNetwork();
            if (dn == null) return null;

            var network = new Network();
            var allNeurons = new List<Neuron>();

            network.LearnRate = dn.LearnRate;
            network.Momentum = dn.Momentum;

            //Input Layer
            foreach (var n in dn.InputLayer)
            {
                var neuron = new Neuron
                {
                    Id = n.Id,
                    Bias = n.Bias,
                    BiasDelta = n.BiasDelta,
                    Gradient = n.Gradient,
                    InputValue = n.InputValue,
                    OutputValue=n.OutputValue,
                    Error=n.Error
                };

                network.InputLayer.Add(neuron);
                allNeurons.Add(neuron);
            }

            //Hidden Layers
            foreach (var layer in dn.HiddenLayers)
            {
                var neurons = new List<Neuron>();
                foreach (var n in layer)
                {
                    var neuron = new Neuron
                    {
                        Id = n.Id,
                        Bias = n.Bias,
                        BiasDelta = n.BiasDelta,
                        Gradient = n.Gradient,
                        InputValue = n.InputValue,
                        OutputValue = n.OutputValue,
                        Error = n.Error
                    };

                    neurons.Add(neuron);
                    allNeurons.Add(neuron);
                }

                network.HiddenLayers.Add(neurons);
            }

            //Export Layer
            foreach (var n in dn.OutputLayer)
            {
                var neuron = new Neuron
                {
                    Id = n.Id,
                    Bias = n.Bias,
                    BiasDelta = n.BiasDelta,
                    Gradient = n.Gradient,
                    InputValue = n.InputValue,
                    OutputValue = n.OutputValue,
                    Error = n.Error
                };

                network.OutputLayer.Add(neuron);
                allNeurons.Add(neuron);
            }

            //Synapses
            foreach (var syn in dn.Synapses)
            {
                var synapse = new Synapse { Id = syn.Id };
                var inputNeuron = allNeurons.First(x => x.Id == syn.InputNeuronId);
                var outputNeuron = allNeurons.First(x => x.Id == syn.OutputNeuronId);
                synapse.InputNeuron = inputNeuron;
                synapse.OutputNeuron = outputNeuron;
                synapse.Weight = syn.Weight;
                synapse.WeightDelta = syn.WeightDelta;

                inputNeuron.OutputSynapses.Add(synapse);
                outputNeuron.InputSynapses.Add(synapse);
            }
            return network;
        }

        public static List<DataSet> ImportDatasets()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Multiselect = false,
                    Title = "Open Dataset File",
                    Filter = "Text File|*.txt;"
                };

               
                    if (dialog.ShowDialog() != true) return null;
                    using (var file = File.OpenText(dialog.FileName))
                    {
                        return JsonConvert.DeserializeObject<List<DataSet>>(file.ReadToEnd());
                    }
               
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static double[] ToDoubleArray(string value, char separator)
        {
            return Array.ConvertAll(value.Split(separator), s => double.Parse(s));
        }

        public static List<DataSet> ImportDatasetsFromCSV(int inputSize,int outputSize)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Multiselect = false,
                    Title = "Open Dataset File",
                    Filter = "Text File|*.txt;"
                };

                if (dialog.ShowDialog() != true) return null;

                List<DataSet> dataSets = new List<DataSet>();

                using (StreamReader sr = new StreamReader(dialog.FileName, Encoding.Default))
                {

                    ProgressDialogResult result = ProgressDialog.ProgressDialog.Execute(Application.Current.MainWindow, "Loading Train Data...", () => {

                        while (sr.Peek()>0)
                        {
                            
                            string temp = sr.ReadLine();
                            double[] all=ToDoubleArray(temp, ',');
                            if(all.Count()!=(inputSize+outputSize))
                            {
                                MessageBox.Show("The lenth of Network is not fit the length of Dataset");
                                break;                              
                            }
                            double[] tar =new double[outputSize];
                            double[] val = new double[inputSize];
                            for(int i=0;i<outputSize;i++)
                            {
                                tar[i] = all[i];
                            }
                            for(int j=0;j<inputSize;j++)
                            {
                                val[j] = all[j+outputSize];
                            }

                            DataSet ds = new DataSet(val,tar);                           
                            dataSets.Add(ds);
                        }
                    });
              

                    if (result.OperationFailed)
                        MessageBox.Show("Loading Train Data failed.");
                    else
                        MessageBox.Show("Loading Train Data successfully.");

                    return dataSets;

                }


            }
            catch (Exception)
            {
                return null;
            }
        }


        private static HelperNetwork GetHelperNetwork()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Multiselect = false,
                    Title = "Open Network File",
                    Filter = "Text File|*.txt;"
                };
               
                if (dialog.ShowDialog() != true) return null;

                using (var file = File.OpenText(dialog.FileName))
                {
                    return JsonConvert.DeserializeObject<HelperNetwork>(file.ReadToEnd());
                }
               
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
