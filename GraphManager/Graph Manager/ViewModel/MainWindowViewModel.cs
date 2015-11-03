﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Graph_Manager.Model;
using PropertyChanged;

namespace Graph_Manager.ViewModel
{
    [ImplementPropertyChanged]
    public class MainWindowViewModel
    {
        public CompositeCollection ObjectCompositeCollection { get; set; }    
        private bool _isImageSelectedLeftButton;
        private bool _isImageSelectedRightButton;
        public Graph Graph { get; set; }
        public static int IdImage { get; set; }
        public static int IdEdge { get; set; }
        public int IndexAction { get; set; }
        public ICommand AddVertexCommand { get; set; }
        public ICommand MoveVertexCommand { get; set; }
        public ICommand DeleteVertexCommand { get; set; }
        public ICommand CanvasMouseLeftButtonDownCommand { get; set; }
        public ICommand CanvasMouseRightButtonDownCommand { get; set; }
        public ICommand LineMouseRightButtonDownCommand { get; set; }

        public bool IsImageSelectedRightButton
        {
            get
            {
                return Graph.Vertexes.Any(v => v.IsMouseRightButtonDown == true);
            }
        }

        public bool IsImageSelectedLeftButton
        {
            get
            {
                return Graph.Vertexes.Any(v => v.IsMouseLeftButtonDown == true);
            }
        }

        private bool AnySelected
        {
            get
            {
                return Graph.Vertexes.Any(v => v.Selected == true);
            }
        }
      
        public MainWindowViewModel()
        {
            Graph = new Graph();
            ObjectCompositeCollection=new CompositeCollection();
            IdImage = IdEdge = 0;
            AddVertexCommand = new RelayCommand(AddVertex, (n) => true);
            MoveVertexCommand = new RelayCommand(MoveVertex, (n) => true);
            DeleteVertexCommand = new RelayCommand(DeleteVertex, (n) => true);
            CanvasMouseLeftButtonDownCommand = new RelayCommand(CanvasMouseLeftButtonDown, (n) => true);
            CanvasMouseRightButtonDownCommand = new RelayCommand(CanvasMouseRightButtonDown, (n) => true);
            LineMouseRightButtonDownCommand=new RelayCommand(LineMouseRightButtonDown, (n) => true);         
        }

        private void LineMouseRightButtonDown(object obj)
        {
            MessageBox.Show("line");
            MessageBox.Show(Convert.ToString((int)obj ));
            var linia = Graph.Edges.First(n => n.IdEdge == (int) obj);
          //  MessageBox.Show(Convert.ToString(linia.StartPoint));
        }

        private void CanvasMouseRightButtonDown(object sender)
        {
            if (IsImageSelectedRightButton == false)
            {
                foreach (var vertex in Graph.Vertexes)
                {
                    vertex.Selected = false;
                }
                MessageBox.Show("canvas prawy click");
            }
        }

        private void CanvasMouseLeftButtonDown(object obj)
        {
            Canvas canvas = obj as Canvas;
            Point point = Mouse.GetPosition(canvas);
            point.X = point.X - Convert.ToDouble(Resources.ImageWidth) / 2;
            point.Y = point.Y - Convert.ToDouble(Resources.ImageHeight) / 2;
      //      MessageBox.Show(Convert.ToString(point));
            //dodaje wolne wierzchołki
            if (IndexAction == 0 && AnySelected == false && IsImageSelectedLeftButton == false)
            {
                Graph.Vertexes.Add(new Vertex()
                {
                    Position = point,
                    IdVertex = IdImage,
                    Margin = new Thickness(point.X, point.Y, 0, 0)
                });
                IdImage++;
                AddToObjectCompositeCollection();
            }

            //dodaje wierzchołek połączony z aktualnie wybranymi (sprawdza czy nie kliknięto na inny wierzchołek,
            //jeśli tak to nie wykonuje operacji)

            else if (IndexAction == 0 && AnySelected == true && IsImageSelectedLeftButton==false)
            {
                var vertex = new Vertex()
                {
                    Position = point,
                    IdVertex = IdImage,
                    Margin = new Thickness(point.X, point.Y, 0, 0)
                };
                IdImage++;
                Graph.Vertexes.Add(vertex);

                AddEdge(vertex);
                AddToObjectCompositeCollection();
            }

            //łaczy wybrane wierzchołki z innym który nie jest zaznaczony
            else if (IndexAction == 0 && AnySelected == true && IsImageSelectedLeftButton==true)
            {
                var vertex = Graph.Vertexes.First(v => v.IsMouseLeftButtonDown==true);
                AddEdge(vertex);
                AddToObjectCompositeCollection();
                Graph.Vertexes.First(v => v.IsMouseLeftButtonDown == true).IsMouseLeftButtonDown = false;
            }

            ////usuwa dowolny wierzchołek
            //else if (IndexAction == 1)// && e.OriginalSource is Image)
            //{
            //    int index = IdImage;
            //    var vertex = Graph.Vertexes.FirstOrDefault(v => v.IdVertex == index);


            //    vertex.ConnectedEdges.ForEach(m =>
            //    {
            //        Graph.Edges.Remove(m);
            //    });

            //    Graph.Vertexes.Remove(vertex);
            //}

            ////usuwa dowolną krawędź
            //else if (IndexAction == 1)//&& e.OriginalSource is Line)
            //{
            //    int index = IdEdge;
            //    var edge = Graph.Edges.FirstOrDefault(v => v.IdEdge == index);


            //    Graph.Edges.Remove(edge);
            //}
        }

        private void AddToObjectCompositeCollection()
        {
            ObjectCompositeCollection.Add(new CollectionContainer() { Collection = Graph.Vertexes });
            ObjectCompositeCollection.Add(new CollectionContainer() { Collection = Graph.Edges });
        }

        private void AddEdge(Vertex vertex)
        {
            foreach (var item in Graph.Vertexes.Where(v => v.Selected == true).ToList())
            {
                var edge = new Edge()
                {
                    StartPoint = item.Position,
                    EndPoint = vertex.Position,            
                    StartVertex = item,
                    EndVertex = vertex,
                    IdEdge = IdEdge
                };
                CalculateStartEndPoint(edge);
                Graph.Edges.Add(edge);
                IdEdge++;
                item.ConnectedEdges.Add(edge);
                item.ConnectedVertexes.Add(vertex);

                vertex.ConnectedEdges.Add(edge);
                vertex.ConnectedVertexes.Add(item);
            }        
        }
        /// <summary>
        /// sluzy do obcinania line, zeby linii nie bylo na obrazku + przemieszczenie do centru obrazka
        /// </summary>
        /// <param name="edge"></param>

        private void CalculateStartEndPoint(Edge edge) 
        {
            double DiagonalBig = Math.Sqrt(Math.Pow(edge.EndPoint.X - edge.StartPoint.X, 2) + Math.Pow(edge.EndPoint.Y - edge.StartPoint.Y, 2));
            Point pointStart = new Point();
            Point pointEnd = new Point();
            pointStart.X = (edge.EndPoint.X - edge.StartPoint.X) / DiagonalBig * Convert.ToUInt32(Resources.Radius) + edge.StartPoint.X;
            pointStart.Y = (edge.EndPoint.Y - edge.StartPoint.Y) / DiagonalBig * Convert.ToUInt32(Resources.Radius) + edge.StartPoint.Y;
            pointEnd.X = edge.EndPoint.X - (edge.EndPoint.X - edge.StartPoint.X) / DiagonalBig * Convert.ToUInt32(Resources.Radius);
            pointEnd.Y = edge.EndPoint.Y - (edge.EndPoint.Y - edge.StartPoint.Y) / DiagonalBig * Convert.ToUInt32(Resources.Radius);
            pointStart.X = pointStart.X + Convert.ToDouble(Resources.ImageWidth) / 2;
            pointStart.Y = pointStart.Y + Convert.ToDouble(Resources.ImageHeight) / 2;
            pointEnd.X = pointEnd.X + Convert.ToDouble(Resources.ImageWidth) / 2;
            pointEnd.Y = pointEnd.Y + Convert.ToDouble(Resources.ImageHeight) / 2;

            edge.StartPoint = pointStart;
            edge.EndPoint = pointEnd;
        }

        private void DeleteVertex(object obj)
        {
            IndexAction = 3;
        }

        private void MoveVertex(object obj)
        {
            IndexAction = 2;
        }

        private void AddVertex(object obj) // 1-dodaj, 2 -move, 3- delete(zamiast comboboxa)
        {
            IndexAction = 1;
        }
    }
}
