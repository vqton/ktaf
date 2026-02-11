package com.tonvq.accountingerp.infrastructure.config;

import io.swagger.v3.oas.models.OpenAPI;
import io.swagger.v3.oas.models.info.Info;
import io.swagger.v3.oas.models.info.Contact;
import io.swagger.v3.oas.models.info.License;
import io.swagger.v3.oas.models.security.SecurityScheme;
import io.swagger.v3.oas.models.security.SecurityRequirement;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * Swagger / OpenAPI 3.0 Configuration
 * API documentation using SpringDoc OpenAPI
 */
@Configuration
public class SwaggerConfig {
    
    @Bean
    public OpenAPI customOpenAPI() {
        return new OpenAPI()
            .info(new Info()
                .title("AccountingERP API")
                .version("1.0.0")
                .description("RESTful API for AccountingERP - TT 99/2025/TT-BTC compliant")
                .contact(new Contact()
                    .name("Vu Quang Ton")
                    .email("vuquangton@outlook.com")
                    .url("https://github.com/vqton/ktaf"))
                .license(new License()
                    .name("Proprietary")
                    .url("https://github.com/vqton/ktaf")))
            .addSecurityItem(new SecurityRequirement().addList("bearerAuth"))
            .getComponents()
            .addSecuritySchemes("bearerAuth", new SecurityScheme()
                .type(SecurityScheme.Type.HTTP)
                .scheme("bearer")
                .bearerFormat("JWT")
                .description("JWT token authentication"))
            .getOpenapi();
    }
}
