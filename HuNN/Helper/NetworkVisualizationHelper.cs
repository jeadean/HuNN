using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace HuNN.Helper
{
    public static class NetworkVisualizationHelper
    {
        public static void ToTreeView(TreeView t, Network nn)
        {
            if (nn == null)
                return;

            t.Items.Clear();
            //t.Nodes.Clear();            
            TreeViewItem root = new TreeViewItem();
            root.Header = "NeuralNetwork";

            TreeViewItem tvInputLayer = new TreeViewItem();
            tvInputLayer.Header = "InputLayer";
            root.Items.Add(tvInputLayer);

            nn.InputLayer.ForEach((neuron) => {
                TreeViewItem nnode = new TreeViewItem();
                nnode.Header = "Neuron";
                nnode.Items.Add("Bias:" + neuron.Bias.ToString());
                nnode.Items.Add("Gradient:" + neuron.Gradient.ToString());
                nnode.Items.Add("InputValue:" + neuron.OutputValue.ToString());
                nnode.Items.Add("OutputValue:" + neuron.OutputValue.ToString());

                neuron.OutputSynapses.ForEach((synapse) => {
                    TreeViewItem snode = new TreeViewItem();
                    snode.Header = "Synapse";
                    snode.Items.Add("Weight:" + synapse.Weight.ToString());
                    snode.Items.Add("WeightDelta:" + synapse.WeightDelta.ToString());
                    nnode.Items.Add(snode);
                });
                tvInputLayer.Items.Add(nnode);
            });


            TreeViewItem tvHiddenLayers = new TreeViewItem();
            tvHiddenLayers.Header = "HiddenLayers";
            root.Items.Add(tvHiddenLayers);

            nn.HiddenLayers.ForEach((layer) => {
                TreeViewItem lnode = new TreeViewItem();
                lnode.Header = "Layer";

                layer.ForEach((neuron)=> {
                    TreeViewItem nnode = new TreeViewItem();
                    nnode.Header = "Neuron";
                    nnode.Items.Add("Bias:" + neuron.Bias.ToString());
                    nnode.Items.Add("Gradient:" + neuron.Gradient.ToString());
                    nnode.Items.Add("InputValue:" + neuron.OutputValue.ToString());
                    nnode.Items.Add("OutputValue:" + neuron.OutputValue.ToString());

                    neuron.OutputSynapses.ForEach((synapse) => {
                        TreeViewItem snode = new TreeViewItem();
                        snode.Header = "Synapse";
                        snode.Items.Add("Weight:" + synapse.Weight.ToString());
                        snode.Items.Add("WeightDelta:" + synapse.WeightDelta.ToString());
                        nnode.Items.Add(snode);
                    });
                    lnode.Items.Add(nnode);
                });             
                tvHiddenLayers.Items.Add(lnode);
            });

            TreeViewItem tvOutputLayer = new TreeViewItem();
            tvOutputLayer.Header = "OutputLayer";
            root.Items.Add(tvOutputLayer);
            nn.OutputLayer.ForEach((neuron) => {
                TreeViewItem nnode = new TreeViewItem();
                nnode.Header = "Neuron";
                nnode.Items.Add("Bias" + neuron.Bias.ToString());
                nnode.Items.Add("Gradient:" + neuron.Gradient.ToString());
                nnode.Items.Add("InputValue" + neuron.OutputValue.ToString());
                nnode.Items.Add("OutputValue" + neuron.OutputValue.ToString());

                neuron.OutputSynapses.ForEach((synapse) => {
                    TreeViewItem snode = new TreeViewItem();
                    snode.Header = "Synapse";
                    snode.Items.Add("Weight" + synapse.Weight.ToString());
                    snode.Items.Add("WeightDelta" + synapse.WeightDelta.ToString());
                    nnode.Items.Add(snode);
                });
                tvOutputLayer.Items.Add(nnode);
            });      

            t.Items.Add(root);
        }

        public static void ToCanvas(Canvas p, Network nn)
        {
            int neuronWidth = 30;
            int neuronDistance = 50;
            int layerDistance = 50;
            int fontSize = 8;
                 
         
        
        }
    }
}
