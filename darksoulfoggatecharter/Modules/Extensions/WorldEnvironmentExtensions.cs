using Godot;
using System.Collections.Generic;

public static class WorldEnvironmentExtensions
{
    public static EnvironmentLerp CreateLerp(this Environment env, Environment other)
    {
        return new EnvironmentLerp(env, other);
    }
}

public class EnvironmentLerp
{
    public Environment Start { get; private set; }
    public Environment End { get; private set; }
    public Environment Result { get; private set; }

    public EnvironmentLerp(Environment start, Environment end)
    {
        Start = start.Duplicate() as Environment;
        End = end.Duplicate() as Environment;
        Result = start.Duplicate() as Environment;
        UpdateLerp(0);
    }

    public void UpdateLerp(float t)
    {
        LerpBackground(t);
        LerpAmbientLight(t);
        LerpReflectedLight(t);
        LerpTonemap(t);
        LerpSSR(t);
        LerpSSAO(t);
        LerpSSIL(t);
        LerpSDFGI(t);
        LerpGlow(t);
        LerpFog(t);
        LerpVolumetricFog(t);
    }

    public void LerpBackground(float t)
    {
        Result.BackgroundMode = t < 0.5f ? Start.BackgroundMode : End.BackgroundMode;

        List<string> properties = new()
        {
            "background_color",
            "background_energy_multiplier",
        };

        LerpProperties("", properties, t);
    }

    public void LerpAmbientLight(float t)
    {
        Result.AmbientLightSource = t < 0.5f ? Start.AmbientLightSource : End.AmbientLightSource;

        List<string> properties = new()
        {
            "ambient_light_color",
            "ambient_light_energy",
        };

        LerpProperties("", properties, t);
    }

    public void LerpReflectedLight(float t)
    {
        Result.ReflectedLightSource = t < 0.5f ? Start.ReflectedLightSource : End.ReflectedLightSource;
    }

    public void LerpTonemap(float t)
    {
        Result.TonemapMode = t < 0.5f ? Start.TonemapMode : End.TonemapMode;

        List<string> properties = new()
        {
            "tonemap_exposure",
        };

        LerpProperties("", properties, t);
    }

    public void LerpSSR(float t)
    {
        Result.SsrEnabled = Start.SsrEnabled || End.SsrEnabled;
        if (!Result.SsrEnabled) return;

        List<string> properties = new()
        {
            "ssr_max_steps",
            "ssr_fade_in",
            "ssr_fade_out",
            "ssr_depth_tolerance",
        };

        LerpProperties("ssr_enabled", properties, t);
    }

    public void LerpSSAO(float t)
    {
        Result.SsaoEnabled = Start.SsaoEnabled || End.SsaoEnabled;
        if (!Result.SsaoEnabled) return;

        List<string> properties = new()
        {
            "ssao_radius",
            "ssao_intensity",
            "ssao_power",
            "ssao_detail",
            "ssao_horizon",
            "ssao_sharpness",
            "ssao_light_affect",
            "ssao_ao_channel_affect",
        };

        LerpProperties("ssao_enabled", properties, t);
    }

    public void LerpSSIL(float t)
    {
        Result.SsilEnabled = Start.SsilEnabled || End.SsilEnabled;
        if (!Result.SsilEnabled) return;

        List<string> properties = new()
        {
            "ssil_radius",
            "ssil_intensity",
            "ssil_sharpness",
            "ssil_normal_rejection",
        };

        LerpProperties("ssil_enabled", properties, t);
    }

    public void LerpSDFGI(float t)
    {
        Result.SdfgiEnabled = Start.SdfgiEnabled || End.SdfgiEnabled;
        if (!Result.SdfgiEnabled) return;

        Result.SdfgiUseOcclusion = t < 0.5f ? Start.SdfgiUseOcclusion : End.SdfgiUseOcclusion;
        Result.SdfgiReadSkyLight = t < 0.5f ? Start.SdfgiReadSkyLight : End.SdfgiReadSkyLight;
        Result.SdfgiYScale = t < 0.5f ? Start.SdfgiYScale : End.SdfgiYScale;

        List<string> properties = new()
        {
            "sdfgi_bounce_feedback",
            "sdfgi_cascades",
            "sdfgi_min_cell_size",
            "sdfgi_cascade0_distance",
            "sdfgi_max_distance",
            "sdfgi_energy",
            "sdfgi_normal_bias",
            "sdfgi_probe_bias",
        };

        LerpProperties("sdfgi_enabled", properties, t);
    }

    public void LerpGlow(float t)
    {
        Result.GlowEnabled = Start.GlowEnabled || End.GlowEnabled;
        if (!Result.GlowEnabled) return;

        Result.GlowNormalized = t < 0.5f ? Start.GlowNormalized : End.GlowNormalized;
        Result.GlowBlendMode = t < 0.5f ? Start.GlowBlendMode : End.GlowBlendMode;

        // GlowMap ?

        List<string> properties = new()
        {
            "glow_levels/1",
            "glow_levels/2",
            "glow_levels/3",
            "glow_levels/4",
            "glow_levels/5",
            "glow_levels/6",
            "glow_levels/7",
            "glow_intensity",
            "glow_strength",
            "glow_bloom",
            "glow_hrd_threshold",
            "glow_hdr_scale",
            "glow_hdr_luminance_cap",
            "glow_map_strength",
        };

        LerpProperties("glow_enabled", properties, t);
    }

    public void LerpFog(float t)
    {
        Result.FogEnabled = Start.FogEnabled || End.FogEnabled;
        if (!Result.FogEnabled) return;

        List<string> properties = new()
        {
            "fog_mode",
            "fog_light_color",
            "fog_light_energy",
            "fog_sun_scatter",
            "fog_density",
            "fog_sky_affect",
            "fog_height",
            "fog_height_density",
            "fog_depth_curve",
            "fog_depth_begin",
            "fog_depth_end",
        };

        LerpProperties("fog_enabled", properties, t);
    }

    public void LerpVolumetricFog(float t)
    {
        Result.VolumetricFogEnabled = Start.VolumetricFogEnabled || End.VolumetricFogEnabled;
        if (!Result.VolumetricFogEnabled) return;

        List<string> properties = new()
        {
            "volumetric_fog_density",
            "volumetric_fog_albedo",
            "volumetric_fog_emission",
            "volumetric_fog_detail_spread",
        };

        LerpProperties("volumetric_fog_enabled", properties, t);
    }

    private void LerpProperties(string property_enabled, List<string> properties, float t)
    {
        foreach (var property in properties)
        {
            var v = LerpProperty(property_enabled, property, t);
            Result.Set(property, v);
        }
    }

    private Variant LerpProperty(string property_enabled, string property_value, float t)
    {
        var b_start = string.IsNullOrEmpty(property_enabled) || Start.Get(property_enabled).AsBool();
        var b_end = string.IsNullOrEmpty(property_enabled) || End.Get(property_enabled).AsBool();
        var v_start = Start.Get(property_value);
        var v_end = End.Get(property_value);
        var start = b_start ? v_start : GetDefaultValue(v_start.VariantType);
        var end = b_end ? v_end : GetDefaultValue(v_end.VariantType);
        var v = Lerp.Variant(start, end, t);
        //Debug.LogMethod($"{property_value}: {v_start.AsString()} -> {v_end.AsString()} = {v.AsString()}");
        return v;
    }

    private Variant GetDefaultValue(Variant.Type type) => type switch
    {
        Variant.Type.Float => 0f,
        Variant.Type.Int => 0,
        Variant.Type.Vector2 => Vector2.Zero,
        Variant.Type.Vector3 => Vector3.Zero,
        Variant.Type.Color => Colors.Transparent,
        _ => throw new System.ArgumentException($"Unsupported type: {type}")
    };
}