﻿/*
The MIT License (MIT)
Copyright (c) 2018 Helix Toolkit contributors
*/

//#define OLD

using SharpDX.Direct3D11;
using System.Collections.Generic;
#if NETFX_CORE
namespace HelixToolkit.UWP.Render
#else
namespace HelixToolkit.Wpf.SharpDX.Render
#endif
{
    using Core;
    using System;

    /// <summary>
    /// 
    /// </summary>
    public class ImmediateContextRenderer : DisposeObject, IRenderer
    {
        private readonly Stack<KeyValuePair<int, IList<IRenderable>>> stackCache1 = new Stack<KeyValuePair<int, IList<IRenderable>>>(20);
        private readonly Stack<KeyValuePair<int, IList<IRenderable2D>>> stack2DCache1 = new Stack<KeyValuePair<int, IList<IRenderable2D>>>(20);
        /// <summary>
        /// Gets or sets the immediate context.
        /// </summary>
        /// <value>
        /// The immediate context.
        /// </value>
        public DeviceContextProxy ImmediateContext { private set; get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmediateContextRenderer"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        public ImmediateContextRenderer(Device device)
        {
            ImmediateContext = Collect(new DeviceContextProxy(device.ImmediateContext));
        }

        private static readonly Func<IRenderable, IRenderContext, bool> updateFunc = (x, context) =>
        {
            x.Update(context);
            return x.IsRenderable;
        };
        /// <summary>
        /// Updates the scene graph.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="renderables">The renderables.</param>
        /// <param name="results"></param>
        /// <returns></returns>
        public virtual void UpdateSceneGraph(IRenderContext context, IList<IRenderable> renderables, IList<IRenderable> results)
        {
            renderables.PreorderDFT(context, updateFunc, results, stackCache1);
        }

        /// <summary>
        /// Updates the scene graph.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="renderables">The renderables.</param>
        /// <returns></returns>
        public void UpdateSceneGraph2D(IRenderContext2D context, IList<IRenderable2D> renderables)
        {
            renderables.PreorderDFTRun((x) =>
            {
                x.Update(context);
                return x.IsRenderable;
            }, stack2DCache1);
        }
        /// <summary>
        /// Updates the global variables. Such as light buffer and global transformation buffer
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="renderables">The renderables.</param>
        /// <param name="parameter">The parameter.</param>
        public virtual void UpdateGlobalVariables(IRenderContext context, IList<IRenderable> renderables, ref RenderParameter parameter)
        {
            if (parameter.RenderLight)
            {
                context.LightScene.LightModels.ResetLightCount();
                for(int i = 0; i < renderables.Count && i < Constants.MaxLights; ++i)
                {
                    renderables[i].Render(context, ImmediateContext);
                }
            }
            if (parameter.UpdatePerFrameData)
            {
                context.UpdatePerFrameData();
            }
        }

        /// <summary>
        /// Renders the scene.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="renderables">The renderables.</param>
        /// <param name="parameter">The parameter.</param>
        public virtual void RenderScene(IRenderContext context, IList<IRenderCore> renderables, ref RenderParameter parameter)
        {
            for (int i = 0; i < renderables.Count; ++i)
            {
                renderables[i].Render(context, ImmediateContext);
            }
        }
        /// <summary>
        /// Updates the no render parallel. <see cref="IRenderer.UpdateNotRenderParallel(IList{IRenderable})"/>
        /// </summary>
        /// <param name="renderables">The renderables.</param>
        /// <returns></returns>
        public virtual void UpdateNotRenderParallel(IList<IRenderable> renderables)
        {
            for(int i = 0; i < renderables.Count; ++i)
            {
                renderables[i].UpdateNotRender();
            }
        }
        /// <summary>
        /// Sets the render targets.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void SetRenderTargets(ref RenderParameter parameter)
        {
            ImmediateContext.DeviceContext.OutputMerger.SetTargets(parameter.DepthStencilView, parameter.RenderTargetView);
            ImmediateContext.DeviceContext.Rasterizer.SetViewport(parameter.ViewportRegion);
            ImmediateContext.DeviceContext.Rasterizer.SetScissorRectangle(parameter.ScissorRegion.Left, parameter.ScissorRegion.Top, 
                parameter.ScissorRegion.Right, parameter.ScissorRegion.Bottom);
        }

        /// <summary>
        /// Render2s the d.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="renderables">The renderables.</param>
        /// <param name="parameter">The parameter.</param>
        public virtual void RenderScene2D(IRenderContext2D context, IList<IRenderable2D> renderables, ref RenderParameter2D parameter)
        {
            for (int i = 0; i < renderables.Count; ++ i)
            {
                renderables[i].Render(context);
            }
        }

        /// <summary>
        /// Renders the pre proc.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="renderables">The renderables.</param>
        /// <param name="parameter">The parameter.</param>
        public virtual void RenderPreProc(IRenderContext context, IList<IRenderCore> renderables, ref RenderParameter parameter)
        {
            RenderScene(context, renderables, ref parameter);
        }

        /// <summary>
        /// Renders the post proc.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="renderables">The renderables.</param>
        /// <param name="parameter">The parameter.</param>
        public virtual void RenderPostProc(IRenderContext context, IList<IRenderCore> renderables, ref RenderParameter parameter)
        {
            RenderScene(context, renderables, ref parameter);
        }
    }

}
